using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.AbstractStorage;

namespace OwlCore.Services
{
    /// <summary>
    /// A base class for getting and setting setting values as properties. Fast access in memory, with data persistence in a file system.
    /// </summary>
    public abstract class SettingsBase : INotifyPropertyChanged
    {
        private readonly IAsyncSerializer<Stream> _settingSerializer;
        private readonly SemaphoreSlim _storageSemaphore = new(1, 1);
        private readonly ConcurrentDictionary<string, (Type Type, object Data)> _runtimeStorage = new();

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
                _runtimeStorage.TryRemove(key, out _);
            }
            else
            {
                _runtimeStorage[key] = (typeof(T), value);
            }

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
            if (_runtimeStorage.TryGetValue(key, out var value))
                return (T)value.Data;

            var fallbackValue = defaultValue();

            // Null values are never stored in runtime or persistent storage.
            if (fallbackValue is not null)
                _runtimeStorage[key] = (typeof(T), fallbackValue);

            return fallbackValue;
        }

        /// <summary>
        /// Sets a settings value to its default.
        /// </summary>
        /// <param name="key">A unique identifier for the setting.</param>
        /// <typeparam name="T">The type of the stored value.</typeparam>
        public void ResetSetting<T>(string key)
        {
            SetSetting<T>(default!, key);
        }

        /// <summary>
        /// Persists all settings from memory onto disk.
        /// </summary>
        /// <remarks>
        /// If any exceptions are thrown while saving a setting, the exception will be swallowed and emitted via <see cref="SaveFailed"/>, and the setting that failed will be excluded from being persisted.
        /// </remarks>
        public virtual async Task SaveAsync(CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;

            await _storageSemaphore.WaitAsync(token);

            foreach (var kvp in _runtimeStorage)
            {
                // Keeping storage of metadata (e.g. original type) separate from actual data allows us to
                // pass the file stream to the serializer directly, without loading the whole thing into memory
                // for modification. 
                // This allows the serializer to load as little or as much data into memory as it needs at a time.
                try
                {
                    var dataFile = await Folder.CreateFileAsync(kvp.Key, CreationCollisionOption.OpenIfExists);
                    var typeFile = await Folder.CreateFileAsync($"{kvp.Key}.Type", CreationCollisionOption.OpenIfExists);

                    if (token.IsCancellationRequested)
                        return;

                    using var serializedRawDataStream = await _settingSerializer.SerializeAsync(kvp.Value.Type, kvp.Value.Data, token);

                    if (token.IsCancellationRequested)
                        return;

                    using var dataFileStream = await dataFile.GetStreamAsync(FileAccessMode.ReadWrite);

                    if (token.IsCancellationRequested)
                        return;

                    dataFileStream.SetLength(serializedRawDataStream.Length);

                    dataFileStream.Seek(0, SeekOrigin.Begin);
                    serializedRawDataStream.Seek(0, SeekOrigin.Begin);

                    await serializedRawDataStream.CopyToAsync(dataFileStream, bufferSize: 81920, token);

                    if (token.IsCancellationRequested)
                        return;

                    // Store the known type for later deserialization. Serializer cannot be relied on for this.
                    var typeContentBytes = Encoding.UTF8.GetBytes(kvp.Value.Type.FullName);
                    using var typeFileStream = await typeFile.GetStreamAsync(FileAccessMode.ReadWrite);
                    typeFileStream.Seek(0, SeekOrigin.Begin);
                    typeFileStream.SetLength(typeContentBytes.Length);

                    await typeFileStream.WriteAsync(typeContentBytes, 0, typeContentBytes.Length, token);
                }
                catch (Exception ex)
                {
                    // Ignore any errors when saving, writing or serializing.
                    // Setting will not be saved.
                    SaveFailed?.Invoke(this, new SettingPersistFailedEventArgs(kvp.Key, ex));
                }
            };

            _storageSemaphore.Release();
        }

        /// <summary>
        /// Loads all settings from disk into memory.
        /// </summary>
        /// <remarks>
        /// If any exceptions are thrown while loading a setting, the exception will be swallowed and emitted via <see cref="LoadFailed"/>, and the current value in memory will be untouched.
        /// </remarks>
        public virtual async Task LoadAsync(CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;

            await _storageSemaphore.WaitAsync(token);

            var files = await Folder.GetFilesAsync();
            var fileData = files as IFileData[] ?? files.ToArray(); // Handle possible multiple enumeration.

            // Remove unpersisted values.
            var unpersistedSettings = _runtimeStorage.Where(x => fileData.All(y => y.Name != x.Key)).ToArray();
            foreach (var setting in unpersistedSettings)
                _runtimeStorage.TryRemove(setting.Key, out _);

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

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(settingDataFile.Name));
                }
                catch (Exception ex)
                {
                    // Ignore any errors when loading, reading or deserializing
                    // Setting will not be loaded into memory.
                    LoadFailed?.Invoke(this, new SettingPersistFailedEventArgs(settingDataFile.Name, ex));
                }
            }

            _storageSemaphore.Release();
        }

        private static async Task<string> ReadFileAsStringAsync(IFileData file, CancellationToken token)
        {
            // Load file
            using var typeFileStream = await file.GetStreamAsync();
            typeFileStream.Seek(0, SeekOrigin.Begin);

            // Convert to raw bytes
            var typeFileBytes = new byte[typeFileStream.Length];
            await typeFileStream.ReadAsync(typeFileBytes, 0, typeFileBytes.Length, token);

            // Read bytes as string
            return Encoding.UTF8.GetString(typeFileBytes);
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raised when an exception is thrown during <see cref="LoadAsync(CancellationToken?)"/>.
        /// </summary>
        public event EventHandler<SettingPersistFailedEventArgs>? LoadFailed;

        /// <summary>
        /// Raised when an exception is thrown during <see cref="SaveAsync(CancellationToken?)"/>.
        /// </summary>
        public event EventHandler<SettingPersistFailedEventArgs>? SaveFailed;
    }

    /// <summary>
    /// Event arguments about a failed persistent save or load in <see cref="SettingsBase"/>.
    /// </summary>
    public class SettingPersistFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="SettingPersistFailedEventArgs"/>.
        /// </summary>
        public SettingPersistFailedEventArgs(string settingName, Exception exception)
        {
            SettingName = settingName;
            Exception = exception;
        }

        /// <summary>
        /// The exception that was raised when attempting to save or load a setting.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The setting which failed to persist.
        /// </summary>
        public string SettingName { get; }
    }
}