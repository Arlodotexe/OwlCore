using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OwlCore.Extensions;
using OwlCore.Services;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Tests.Services;

[TestClass]
public class SettingsBaseTests
{
    [TestMethod, Timeout(2000)]
    public Task GetFallbackValue()
    {
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
        var settings = new TestSettings(settingsStore);

        Assert.AreEqual(settings.StringData, "Default value");
        return Task.CompletedTask;
    }

    [TestMethod, Timeout(2000)]
    public Task SetAndGetValueInMemory()
    {
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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

        // Reset value
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

    [TestMethod]
    public void ResetSettingNotifiesPropertyChanged()
    {
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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

        // Reset value
        settings.ResetSetting(nameof(settings.StringData));
        Assert.AreEqual(TestSettings.StringData_DefaultValue, settings.StringData);

        // Ensure only the assigned property changed.
        Assert.AreEqual(2, changedProperties.Count);
        Assert.AreEqual(nameof(settings.StringData), changedProperties[0]);
        Assert.AreEqual(nameof(settings.StringData), changedProperties[1]);

        settings.PropertyChanged -= OnChanged;

        void OnChanged(object? sender, PropertyChangedEventArgs e) =>
            changedProperties.Add(e.PropertyName ?? throw new InvalidOperationException());
    }

    [TestMethod]
    public void ResetAllSettingsNotifiesPropertyChanged()
    {
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
        var settings = new TestSettings(settingsStore);

        var intermediateStringValue = "Intermediate value";
        var intermediateCompositeValue = new TestSettings.CompositeTestSetting(new byte[] { 0x41, 0x41 }, intermediateStringValue);

        // Initial value must not equal new value for test to be valid.
        Assert.AreNotEqual(intermediateStringValue, settings.StringData);
        Assert.AreNotEqual(intermediateStringValue.Length, settings.StringData.Length);
        Assert.AreNotEqual(intermediateCompositeValue, settings.CompositeData);

        // Track changed properties
        var changedProperties = new List<string>();
        settings.PropertyChanged += OnChanged;

        // Assign a new value
        settings.StringData = intermediateStringValue;
        Assert.AreEqual(intermediateStringValue, settings.StringData);

        settings.CompositeData = intermediateCompositeValue;
        Assert.AreEqual(intermediateCompositeValue, settings.CompositeData);

        // Get a value to force evaluation of the default lambda
        _ = settings.State;

        // Reset value
        settings.ResetAllSettings();
        Assert.AreEqual(TestSettings.StringData_DefaultValue, settings.StringData);
        Assert.AreEqual(TestSettings.State_DefaultValue, settings.State);

        // Ensure properties changed a second time due to reset.
        Assert.AreEqual(2 + 2 + 1, changedProperties.Count);
        Assert.AreEqual(2, changedProperties.Count(p => p == nameof(settings.StringData)));
        Assert.AreEqual(2, changedProperties.Count(p => p == nameof(settings.CompositeData)));
        Assert.AreEqual(1, changedProperties.Count(p => p == nameof(settings.State)));

        settings.PropertyChanged -= OnChanged;

        void OnChanged(object? sender, PropertyChangedEventArgs e) =>
            changedProperties.Add(e.PropertyName ?? throw new InvalidOperationException());
    }

    [TestMethod, Timeout(2000)]
    public void NoDeadlockWhileGetSettingDuringPropertyChanged()
    {
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        var settingsStore = new MemoryFolder(id: "Settings", name: "Settings");
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
        public const bool State_DefaultValue = false;

        public TestSettings(IModifiableFolder folder)
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
            get => GetSetting(() => State_DefaultValue);
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
}