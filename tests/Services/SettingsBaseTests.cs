using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractStorage;
using OwlCore.Services;

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

        settings.StringData = newValue;
        settings.CompositeData.Label = newValue;

        await settings.SaveAsync();
        await settings.LoadAsync();

        Assert.AreEqual(newValue, settings.CompositeData.Label);
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

    [TestMethod, Timeout(2000)]
    public void NoDeadlockWhileGetSettingDuringPropertyChanged()
    {
        var settingsStore = new MockFolder(name: "Settings");
        var settings = new TestSettings(settingsStore);
        var newValue = nameof(SettingsBaseTests);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(newValue, settings.StringData);

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
        public TestSettings(IFolderData folder)
            : base(folder, MockSerializer.Singleton)
        {
        }

        public string StringData
        {
            get => GetSetting(() => "Default value");
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

    private class MockSerializer : IAsyncSerializer<Stream>
    {
        public static MockSerializer Singleton { get; } = new();

        public async Task<Stream> SerializeAsync<T>(T data, CancellationToken? cancellationToken = null)
        {
            var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, data,
                cancellationToken: cancellationToken ?? CancellationToken.None);
            return stream;
        }

        public async Task<Stream> SerializeAsync(Type inputType, object data,
            CancellationToken? cancellationToken = null)
        {
            var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, data, inputType,
                cancellationToken: cancellationToken ?? CancellationToken.None);
            return stream;
        }

        public Task<TResult> DeserializeAsync<TResult>(Stream serialized, CancellationToken? cancellationToken = null)
        {
            return System.Text.Json.JsonSerializer.DeserializeAsync<TResult>(serialized).AsTask()!;
        }

        public Task<object> DeserializeAsync(Type returnType, Stream serialized,
            CancellationToken? cancellationToken = null)
        {
            return System.Text.Json.JsonSerializer.DeserializeAsync(serialized, returnType).AsTask()!;
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
            var file = new MockFile(desiredName);
            _files.Add(file);
            return Task.FromResult<IFileData>(file);
        }

        public Task<IFileData> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            var file = new MockFile(desiredName);
            _files.Add(file);
            return Task.FromResult<IFileData>(file);
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

        public override void Write(byte[] buffer, int offset, int count) =>
            _underlyingStream.Write(buffer, offset, count);

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