using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.AbstractStorage;
using OwlCore.Extensions;

namespace OwlCore.Services
{
    /// <summary>
    /// A base class for getting and setting setting values as properties. Fast access in memory, with data persistence in a file system.
    /// </summary>
    public abstract class SettingsBase : INotifyPropertyChanged
    {
        private readonly IAsyncSerializer<Stream> _settingSerializer;
        private readonly SemaphoreSlim _runtimeStorageMutex = new(1, 1);
        private readonly Dictionary<string, (Type Type, object Data)> _runtimeStorage = new();

        /// <summary>
        /// Creates a new instance of <see cref="SettingsBase"/>.
        /// </summary>
        /// <param name="folder">The folder where settings are stored.</param>
        /// <param name="settingSerializer">The serializer used to serialize and deserialize settings to and from disk.</param>
        protected SettingsBase(IFolderData folder, IAsyncSerializer<Stream> settingSerializer)
        {
            _settingSerializer = settingSerializer;
            Folder = folder;
        }

        /// <summary>
        /// A folder abstraction where the settings can be stored and persisted.
        /// </summary>
        public IFolderData Folder { get; }

        /// <summary>
        /// Stores a settings value.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="key">A unique identifier for this setting.</param>
        /// <typeparam name="T">The type of the stored value.</typeparam>
        protected void SetSetting<T>(T value, [CallerMemberName] string key = "")
        {
            if (value is null)
            {
                _runtimeStorageMutex.Wait();
                _runtimeStorage.Remove(key);
                _runtimeStorageMutex.Release();
                return;
            }

            _runtimeStorageMutex.Wait();
            _runtimeStorage[key] = (typeof(T), value);
            _runtimeStorageMutex.Release();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }

        /// <summary>
        /// Gets a settings value.
        /// </summary>
        /// <param name="defaultValue">A <see cref="Func{TResult}"/> that returns a fallback value to use when the setting is retrieved but was no value was ever stored.</param>
        /// <param name="key">A unique identifier for this setting.</param>
        /// <typeparam name="T">The type of the stored value.</typeparam>
        protected T GetSetting<T>(Func<T> defaultValue, [CallerMemberName] string key = "")
        {
            _runtimeStorageMutex.Wait();
            if (_runtimeStorage.TryGetValue(key, out var value))
            {
                _runtimeStorageMutex.Release();
                return (T)value.Data;
            }

            var fallbackValue = defaultValue();

            // Null values are never stored in runtime or persistent storage.
            if (fallbackValue is not null)
                _runtimeStorage[key] = (typeof(T), fallbackValue);

            _runtimeStorageMutex.Release();
            return fallbackValue;
        }

        /// <summary>
        /// Persists all settings from memory onto disk.
        /// </summary>
        public virtual async Task SaveAsync(CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;

            using (await Flow.EasySemaphore(_runtimeStorageMutex, token))
            {
                await _runtimeStorage.InParallel(async kvp =>
                {
                    // Keeping storage of metadata (e.g. original type) separate from actual data allows us to
                    // pass the file stream to the serializer directly, without loading the whole thing into memory
                    // for modification. 
                    // This allows the serializer to load as little or as much data into memory as it needs at a time.
                    var dataFile = await Folder.CreateFileAsync(kvp.Key, CreationCollisionOption.OpenIfExists);
                    var typeFile = await Folder.CreateFileAsync($"{kvp.Key}.Type", CreationCollisionOption.OpenIfExists);
                    if (token.IsCancellationRequested)
                        return;

                    using var serializedRawDataStream = await _settingSerializer.SerializeAsync(kvp.Value.Type, kvp.Value.Data, token);
                    serializedRawDataStream.Position = 0;

                    if (token.IsCancellationRequested)
                        return;

                    using var dataFileStream = await dataFile.GetStreamAsync(FileAccessMode.ReadWrite);
                    dataFileStream.Position = 0;

                    if (token.IsCancellationRequested)
                        return;

                    await serializedRawDataStream.CopyToAsync(dataFileStream, bufferSize: 81920, token);
                    if (token.IsCancellationRequested)
                        return;

                    // Store the known type for later deserialization. Serializer cannot be relied on for this.
                    var typeContentBytes = Encoding.UTF8.GetBytes(kvp.Value.Type.AssemblyQualifiedName);
                    using var typeFileStream = await typeFile.GetStreamAsync(FileAccessMode.ReadWrite);
                    await typeFileStream.WriteAsync(typeContentBytes, 0, typeContentBytes.Length, token);
                });
            }
        }

        /// <summary>
        /// Loads all settings from disk into memory.
        /// </summary>
        public virtual async Task LoadAsync(CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;

            var files = await Folder.GetFilesAsync();
            var fileData = files as IFileData[] ?? files.ToArray(); // Handle possible multiple enumeration.

            // Remove unpersisted values.
            var unpersistedSettings = _runtimeStorage.Where(x => fileData.All(y => y.Name != x.Key)).ToArray();
            foreach (var setting in unpersistedSettings)
                _runtimeStorage.Remove(setting.Key);

            // Filter out non Type files, so only raw data files remain.
            var nonTypeFiles = fileData.Where(x => !x.Name.Contains("Type"));

            // Load persisted values.
            foreach (var settingDataFile in nonTypeFiles)
            {
                var typeFile = fileData.FirstOrDefault(x => x.Name == $"{settingDataFile.Name}.Type");
                if (typeFile is null)
                    continue; // Type file may be missing or deleted.

                try
                {
                    using (await Flow.EasySemaphore(_runtimeStorageMutex, token))
                    {
                        using var settingDataStream = await settingDataFile.GetStreamAsync();
                        settingDataStream.Position = 0;

                        var typeFileContentString = await ReadFileAsStringAsync(typeFile, token);
                        if (string.IsNullOrWhiteSpace(typeFileContentString))
                            continue;

                        // Get original type
                        var originalType = Type.GetType(typeFileContentString);
                        if (originalType is null)
                            continue;

                        // Deserialize data as original type.
                        var settingData = await _settingSerializer.DeserializeAsync(originalType, settingDataStream, token);

                        _runtimeStorage[settingDataFile.Name] = (originalType, settingData);
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(settingDataFile.Name));
                }
                catch
                {
                    // Ignore any errors when loading, reading or deserializing
                    // Setting will not be loaded into memory.
                }
            }
        }

        private static async Task<string> ReadFileAsStringAsync(IFileData file, CancellationToken token)
        {
            // Load file
            using var typeFileStream = await file.GetStreamAsync();
            typeFileStream.Position = 0;

            // Convert to raw bytes
            var typeFileBytes = new byte[typeFileStream.Length];
            await typeFileStream.ReadAsync(typeFileBytes, 0, typeFileBytes.Length, token);

            // Read bytes as string
            return Encoding.UTF8.GetString(typeFileBytes);
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}