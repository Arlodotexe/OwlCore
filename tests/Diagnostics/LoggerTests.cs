using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Diagnostics;

namespace OwlCore.Tests.Diagnostics;

[TestClass]
public class LoggerTests
{
    private static readonly SemaphoreSlim _testSemaphore = new(1, 1);
    private const string FileName = "LoggerTests.cs";
    
    [TestMethod]
    public async Task LogCritical()
    {
        await _testSemaphore.WaitAsync();
        var msg = Guid.NewGuid().ToString();

        var messageLoggedTask = OwlCore.Flow.EventAsTask<LoggerMessageEventArgs>(x => Logger.MessageReceived += x, x => Logger.MessageReceived -= x, CancellationToken.None);
        Logger.LogCritical(msg);

        var res = await messageLoggedTask;
        Assert.AreEqual(msg, res?.Result.Message);
        Assert.AreEqual(nameof(LogCritical), res?.Result.CallerMemberName);
        Assert.AreEqual(LogLevel.Critical, res?.Result.Level);
        Assert.IsTrue(res?.Result.CallerFilePath.EndsWith(FileName));
        _testSemaphore.Release();
    }
    
    [TestMethod]
    public async Task LogError()
    {
        await _testSemaphore.WaitAsync();
        var msg = Guid.NewGuid().ToString();

        var messageLoggedTask = OwlCore.Flow.EventAsTask<LoggerMessageEventArgs>(x => Logger.MessageReceived += x, x => Logger.MessageReceived -= x, CancellationToken.None);
        Logger.LogError(msg);

        var res = await messageLoggedTask;
        Assert.AreEqual(msg, res?.Result.Message);
        Assert.AreEqual(nameof(LogError), res?.Result.CallerMemberName);
        Assert.AreEqual(LogLevel.Error, res?.Result.Level);
        Assert.IsTrue(res?.Result.CallerFilePath.EndsWith(FileName));
        _testSemaphore.Release();
    }
    
    [TestMethod]
    public async Task LogErrorWithException()
    {
        await _testSemaphore.WaitAsync();
        var msg = Guid.NewGuid().ToString();
        var ex = new ArgumentNullException();
        
        var messageLoggedTask = OwlCore.Flow.EventAsTask<LoggerMessageEventArgs>(x => Logger.MessageReceived += x, x => Logger.MessageReceived -= x, CancellationToken.None);
        Logger.LogError(msg, ex);

        var res = await messageLoggedTask;
        Assert.AreEqual(msg, res?.Result.Message);
        Assert.AreEqual(nameof(LogErrorWithException), res?.Result.CallerMemberName);
        Assert.AreEqual(LogLevel.Error, res?.Result.Level);
        Assert.AreSame(ex, res?.Result.Exception);
        Assert.IsTrue(res?.Result.CallerFilePath.EndsWith(FileName));
        _testSemaphore.Release();
    }
        
    [TestMethod]
    public async Task LogWarning()
    {
        await _testSemaphore.WaitAsync();
        var msg = Guid.NewGuid().ToString();

        var messageLoggedTask = OwlCore.Flow.EventAsTask<LoggerMessageEventArgs>(x => Logger.MessageReceived += x, x => Logger.MessageReceived -= x, CancellationToken.None);
        Logger.LogWarning(msg);

        var res = await messageLoggedTask;
        Assert.AreEqual(msg, res?.Result.Message);
        Assert.AreEqual(nameof(LogWarning), res?.Result.CallerMemberName);
        Assert.AreEqual(LogLevel.Warning, res?.Result.Level);
        Assert.IsTrue(res?.Result.CallerFilePath.EndsWith(FileName));
        _testSemaphore.Release();
    }
        
    [TestMethod]
    public async Task LogInformation()
    {
        await _testSemaphore.WaitAsync();
        var msg = Guid.NewGuid().ToString();

        var messageLoggedTask = OwlCore.Flow.EventAsTask<LoggerMessageEventArgs>(x => Logger.MessageReceived += x, x => Logger.MessageReceived -= x, CancellationToken.None);
        Logger.LogInformation(msg);

        var res = await messageLoggedTask;
        Assert.AreEqual(msg, res?.Result.Message);
        Assert.AreEqual(nameof(LogInformation), res?.Result.CallerMemberName);
        Assert.AreEqual(LogLevel.Information, res?.Result.Level);
        Assert.IsTrue(res?.Result.CallerFilePath.EndsWith(FileName));
        _testSemaphore.Release();
    }
        
    [TestMethod]
    public async Task LogTrace()
    {
        await _testSemaphore.WaitAsync();
        var msg = Guid.NewGuid().ToString();

        var messageLoggedTask = OwlCore.Flow.EventAsTask<LoggerMessageEventArgs>(x => Logger.MessageReceived += x, x => Logger.MessageReceived -= x, CancellationToken.None);
        Logger.LogTrace(msg);

        var res = await messageLoggedTask;
        Assert.AreEqual(msg, res?.Result.Message);
        Assert.AreEqual(nameof(LogTrace), res?.Result.CallerMemberName);
        Assert.AreEqual(LogLevel.Trace, res?.Result.Level);
        Assert.IsTrue(res?.Result.CallerFilePath.EndsWith(FileName));
        _testSemaphore.Release();
    }
        
    [TestMethod]
    public Task Log()
    {
        return Task.CompletedTask;
    }
}