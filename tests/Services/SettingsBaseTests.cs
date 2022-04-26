using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractStorage;
using OwlCore.Services;
using OwlCore.Extensions;
using Newtonsoft.Json;

namespace OwlCore.Tests.Services;

[TestClass]
public class SettingsBaseTests
{
    [TestMethod, Timeout(2000)]
    public Task GetFallbackValue()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        Assert.AreEqual(settings.StringData, "Default value");
        return Task.CompletedTask;
    }

    [TestMethod, Timeout(2000)]
    public Task SetAndGetValueInMemory()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        const string newValue = nameof(SetAndGetValueInMemory);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        settings.StringData = newValue;

        Assert.AreEqual(newValue, settings.StringData);
        return Task.CompletedTask;
    }

    [TestMethod, Timeout(2000)]
    public async Task SaveAndLoadAsync()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        const string newValue = nameof(SetAndGetValueInMemory);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        settings.StringData = newValue;
        settings.CompositeData.Label = newValue;

        await settings.SaveAsync();
        await settings.LoadAsync();

        Assert.AreEqual(newValue, settings.CompositeData.Label);
        Assert.AreEqual(newValue, settings.StringData);
    }

    [TestMethod, Timeout(2000)]
    public async Task SaveValueSaveNewShorterValueThenLoad()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        settings.LoadFailed += (s, e) => Assert.Fail(e.Exception.Message);
        settings.SaveFailed += (s, e) => Assert.Fail(e.Exception.Message);

        // Set long value
        settings.StringData = "yy";

        // Save default values to disk
        await settings.SaveAsync();

        const string newValue = "x";

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.IsTrue(newValue.Length < settings.StringData.Length);

        settings.StringData = newValue;

        // Save new values to disk
        await settings.SaveAsync();

        // Load new values back from disk
        await settings.LoadAsync();

        Assert.AreEqual(newValue, settings.StringData);
    }

    [TestMethod, Timeout(2000)]
    public async Task SaveAndLoadAsyncWithNewSettingsInstanceWithNonSettingFilesInFolder()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        const string newValue = nameof(SetAndGetValueInMemory);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        settings.StringData = newValue;
        settings.CompositeData.Label = newValue;
        Assert.AreEqual(newValue, settings.StringData);

        await settings.SaveAsync();

        // Reusing the same settings store should allow us to read the values we just saved.
        var settings2 = new TestSettings(settingsStore);

        // Initial value must not equal new value until loaded.
        Assert.AreNotEqual(newValue, settings2.StringData);

        await settingsStore.CreateFileAsync("JunkFile");
        await settings2.LoadAsync();

        Assert.AreEqual(newValue, settings.CompositeData.Label);
        Assert.AreEqual(newValue, settings2.StringData);
    }

    [TestMethod, Timeout(2000)]
    public async Task SaveAndLoadAsyncWithNewSettingsInstance()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        const string newValue = nameof(SetAndGetValueInMemory);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        settings.StringData = newValue;
        settings.CompositeData.Label = newValue;
        Assert.AreEqual(newValue, settings.StringData);

        await settings.SaveAsync();

        // Reusing the same settings store should allow us to read the values we just saved.
        var settings2 = new TestSettings(settingsStore);

        // Initial value must not equal new value until loaded.
        Assert.AreNotEqual(newValue, settings2.StringData);

        await settings2.LoadAsync();

        Assert.AreEqual(newValue, settings.CompositeData.Label);
        Assert.AreEqual(newValue, settings2.StringData);
    }

    [TestMethod, Timeout(2000)]
    public async Task LoadWithoutSaveDiscardsInMemoryValue()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        var newValue = nameof(SetAndGetValueInMemory);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        settings.StringData = newValue;
        Assert.AreEqual(newValue, settings.StringData);

        await settings.LoadAsync();

        Assert.AreNotEqual(newValue, settings.StringData);
    }

    [TestMethod, Timeout(2000)]
    public async Task SaveNewValueThenLoadNotifiesPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        var newValue = nameof(SetAndGetValueInMemory);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        // Assign a value to save.
        settings.StringData = newValue;
        Assert.AreEqual(newValue, settings.StringData);
        await settings.SaveAsync();

        // Assign value different than what was saved.
        settings.StringData = string.Empty;
        Assert.AreNotEqual(newValue, settings.StringData);

        // Track changed properties
        var changedProperties = new List<string>();
        settings.PropertyChanged += OnChanged;

        // Reload saved value.
        await settings.LoadAsync();

        // Ensure only the assigned property changed.
        Assert.AreEqual(1, changedProperties.Count);
        Assert.AreEqual(nameof(settings.StringData), changedProperties[0]);

        settings.PropertyChanged -= OnChanged;

        void OnChanged(object? sender, PropertyChangedEventArgs e) =>
            changedProperties.Add(e.PropertyName ?? throw new InvalidOperationException());
    }

    [TestMethod]
    public void SetSettingNotifiesPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        var newValue = nameof(SetAndGetValueInMemory);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        // Track changed properties
        var changedProperties = new List<string>();
        settings.PropertyChanged += OnChanged;

        // Assign a new value
        settings.StringData = newValue;
        Assert.AreEqual(newValue, settings.StringData);

        // Ensure only the assigned property changed.
        Assert.AreEqual(1, changedProperties.Count);
        Assert.AreEqual(nameof(settings.StringData), changedProperties[0]);

        settings.PropertyChanged -= OnChanged;

        void OnChanged(object? sender, PropertyChangedEventArgs e) =>
            changedProperties.Add(e.PropertyName ?? throw new InvalidOperationException());
    }

    [TestMethod]
    public void SetSettingToNullNotifiesPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        var intermediateValue = "Intermediate value";

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(intermediateValue, settings.StringData);
        Assert.AreNotEqual(intermediateValue.Length, settings.StringData.Length);

        // Track changed properties
        var changedProperties = new List<string>();
        settings.PropertyChanged += OnChanged;

        // Assign a new value
        settings.StringData = intermediateValue;
        Assert.AreEqual(intermediateValue, settings.StringData);

        // Rest value
        settings.StringData = null!;
        Assert.AreEqual(TestSettings.StringData_DefaultValue, settings.StringData);

        // Ensure only the assigned property changed.
        Assert.AreEqual(2, changedProperties.Count);
        Assert.AreEqual(nameof(settings.StringData), changedProperties[0]);
        Assert.AreEqual(nameof(settings.StringData), changedProperties[1]);

        settings.PropertyChanged -= OnChanged;

        void OnChanged(object? sender, PropertyChangedEventArgs e) =>
            changedProperties.Add(e.PropertyName ?? throw new InvalidOperationException());
    }
    [TestMethod, Timeout(2000)]
    public void NoDeadlockWhileGetSettingDuringPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);
        var newValue = nameof(SettingsBaseTests);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);
        Assert.AreNotEqual(newValue.Length, settings.StringData.Length);

        settings.PropertyChanged += OnChanged;

        // Assign a new value
        settings.StringData = newValue;
        Assert.AreEqual(newValue, settings.StringData);

        settings.PropertyChanged -= OnChanged;

        void OnChanged(object? sender, PropertyChangedEventArgs e) =>
            Assert.AreEqual(settings.StringData, settings.StringData);
    }

    [TestMethod, Timeout(2000)]
    public void NoDeadlockWhileSetSettingDuringPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);

        settings.PropertyChanged += OnChanged;

        // Assign a new value
        settings.State = true;

        settings.PropertyChanged -= OnChanged;

        void OnChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (settings.State)
                settings.State = false;
        }
    }

    [TestMethod, Timeout(2000)]
    public async Task NoDeadlockWhileSetSettingDuringPropertyChangedThenSavingDuringPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);
        var savedTaskCompletionSource = new TaskCompletionSource();

        settings.PropertyChanged += OnChanged;

        // Assign a new value
        settings.State = true;
        await savedTaskCompletionSource.Task;

        settings.PropertyChanged -= OnChanged;

        async void OnChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (settings.State)
            {
                // Trips another PropertyChanged call
                settings.State = false;
            }

            if (!settings.State)
            {
                await settings.SaveAsync();
                savedTaskCompletionSource.SetResult();
            }
        }
    }

    [TestMethod, Timeout(2000)]
    public async Task NoDeadlockWhileSavingDuringPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);
        var newValue = nameof(SettingsBaseTests);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);

        var savedTaskCompletionSource = new TaskCompletionSource();

        settings.PropertyChanged += OnChanged;

        // Assign a new value
        settings.StringData = newValue;
        await savedTaskCompletionSource.Task;

        settings.PropertyChanged -= OnChanged;

        async void OnChanged(object? sender, PropertyChangedEventArgs e)
        {
            await settings.SaveAsync();
            savedTaskCompletionSource.SetResult();
        }
    }

    private class TestSettings : SettingsBase
    {
        public const string StringData_DefaultValue = "Default value";

        public TestSettings(IFolderData folder)
            : base(folder, NewtonsoftStreamSerializer.Singleton)
        {
        }

        public string StringData
        {
            get => GetSetting(() => StringData_DefaultValue);
            set => SetSetting(value);
        }

        public bool State
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        public CompositeTestSetting CompositeData
        {
            get => GetSetting(() => new CompositeTestSetting(Array.Empty<byte>(), string.Empty));
            set => SetSetting(value);
        }

        public class CompositeTestSetting
        {
            public CompositeTestSetting(byte[] Data, string Label)
            {
                this.Data = Data;
                this.Label = Label;
            }

            public byte[] Data { get; set; }
            public string Label { get; set; }
        }
    }

    private class NewtonsoftStreamSerializer : IAsyncSerializer<Stream>, ISerializer<Stream>
    {
        /// <summary>
        /// A singleton instance for <see cref="NewtonsoftStreamSerializer"/>.
        /// </summary>
        public static NewtonsoftStreamSerializer Singleton { get; } = new();

        /// <inheritdoc />
        public Task<Stream> SerializeAsync<T>(T data, CancellationToken? cancellationToken = null) => Task.Run(() => Serialize(data), cancellationToken ?? CancellationToken.None);

        /// <inheritdoc />
        public Task<Stream> SerializeAsync(Type inputType, object data, CancellationToken? cancellationToken = null) => Task.Run(() => Serialize(inputType, data), cancellationToken ?? CancellationToken.None);

        /// <inheritdoc />
        public Task<TResult> DeserializeAsync<TResult>(Stream serialized, CancellationToken? cancellationToken = null) => Task.Run(() => Deserialize<TResult>(serialized), cancellationToken ?? CancellationToken.None);

        /// <inheritdoc />
        public Task<object> DeserializeAsync(Type returnType, Stream serialized, CancellationToken? cancellationToken = null) => Task.Run(() => Deserialize(returnType, serialized), cancellationToken ?? CancellationToken.None);

        /// <inheritdoc />
        public Stream Serialize<T>(T data)
        {
            var res = JsonConvert.SerializeObject(data, typeof(T), null);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        /// <inheritdoc />
        public Stream Serialize(Type type, object data)
        {
            var res = JsonConvert.SerializeObject(data, type, null);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        /// <inheritdoc />
        public TResult Deserialize<TResult>(Stream serialized)
        {
            serialized.Position = 0;
            var str = Encoding.UTF8.GetString(serialized.ToBytes());
            return (TResult)JsonConvert.DeserializeObject(str, typeof(TResult))!;
        }

        /// <inheritdoc />
        public object Deserialize(Type type, Stream serialized)
        {
            serialized.Position = 0;
            var str = Encoding.UTF8.GetString(serialized.ToBytes());
            return JsonConvert.DeserializeObject(str, type)!;
        }
    }

    public class MockFile : IFileData
    {
        private readonly MemoryStream _mockStream = new();

        public MockFile(string name)
        {
            Id = name;
            Path = name;
            Name = name;
            DisplayName = name;
            FileExtension = string.Empty;
        }

        public string? Id { get; }
        public string Path { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public string FileExtension { get; }

        public IFileDataProperties Properties { get; set; }

        public Task<IFolderData> GetParentAsync()
        {
            throw new NotSupportedException();
        }

        public Task<Stream> GetStreamAsync(FileAccessMode accessMode = FileAccessMode.Read)
        {
            // A new non-disposing stream must be created over the actual content each time.
            // If we returned the raw stream, the data would be deleted when the stream is disposed.
            return Task.FromResult<Stream>(new NonDisposingProxyStream(_mockStream));
        }

        public Task Delete()
        {
            throw new NotSupportedException();
        }

        public Task WriteAllBytesAsync(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public Task<Stream> GetThumbnailAsync(ThumbnailMode thumbnailMode, uint requiredSize)
        {
            throw new NotSupportedException();
        }
    }

    public class MockFolder : IFolderData
    {
        private ConcurrentBag<IFileData> _files = new();

        public MockFolder(string name)
        {
            Name = name;
            Path = name;
            Id = name;
        }

        public string? Id { get; }
        public string Name { get; }
        public string Path { get; }

        public Task<IFolderData?> GetParentAsync()
        {
            throw new NotSupportedException();
        }

        public Task<IFolderData> CreateFolderAsync(string desiredName)
        {
            throw new NotSupportedException();
        }

        public Task<IFolderData> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            throw new NotSupportedException();
        }

        public Task<IFileData> CreateFileAsync(string desiredName)
        {
            return CreateFileAsync(desiredName, CreationCollisionOption.FailIfExists);
        }

        public Task<IFileData> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            if (_files.FirstOrDefault(x => x.Name == desiredName) is IFileData file)
            {
                if (options == CreationCollisionOption.OpenIfExists)
                    return Task.FromResult(file);

                if (options == CreationCollisionOption.FailIfExists)
                    throw new Exception();

                throw new NotSupportedException($"Support for {options} not added.");
            }

            var newFile = new MockFile(desiredName);
            _files.Add(newFile);
            return Task.FromResult<IFileData>(newFile);
        }

        public Task<IFolderData?> GetFolderAsync(string name)
        {
            throw new NotSupportedException();
        }

        public Task<IFileData?> GetFileAsync(string name)
        {
            return Task.FromResult(_files.FirstOrDefault(x => x.Name == name));
        }

        public Task<IEnumerable<IFolderData>> GetFoldersAsync()
        {
            throw new NotSupportedException();
        }

        public Task<IEnumerable<IFileData>> GetFilesAsync()
        {
            return Task.FromResult<IEnumerable<IFileData>>(_files.ToArray());
        }

        public Task DeleteAsync()
        {
            throw new NotSupportedException();
        }

        public Task EnsureExists()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Proxies another stream for reading and writing, but does not dispose it.
    /// </summary>
    /// <remarks>
    /// Needed to properly emulate file storage in memory. If we returned the raw MemoryStream, it would effectively delete the data.
    /// </remarks>
    public class NonDisposingProxyStream : Stream
    {
        private readonly Stream _underlyingStream;

        public NonDisposingProxyStream(Stream underlyingStream)
        {
            _underlyingStream = underlyingStream;
        }

        public override void Flush() => _underlyingStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _underlyingStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _underlyingStream.Seek(offset, origin);

        public override void SetLength(long value) => _underlyingStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _underlyingStream.Write(buffer, offset, count);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _underlyingStream.WriteAsync(buffer, offset, count, cancellationToken);

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => _underlyingStream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override bool CanRead => _underlyingStream.CanRead;

        public override bool CanSeek => _underlyingStream.CanSeek;

        public override bool CanWrite => _underlyingStream.CanWrite;

        public override long Length => _underlyingStream.Length;

        public override long Position
        {
            get => _underlyingStream.Position;
            set => _underlyingStream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override ValueTask DisposeAsync() => default;
    }
}