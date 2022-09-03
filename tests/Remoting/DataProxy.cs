using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Extensions;
using OwlCore.Remoting;
using OwlCore.Remoting.Transfer;
using OwlCore.Tests.Remoting.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlCore.Tests.Remoting
{
    [TestClass]
    public class DataProxy
    {
        [TestMethod]
        [Timeout(5000)]
        public async Task TestPublishAsync()
        {
            var instance = new DataProxyRemotingTestClass(RemotingMode.Host);
            var taskCompletionSource = new TaskCompletionSource<object?>();

            WeaveLoopbackHandlers(instance.MemberRemote);

            instance.MemberRemote.MessageSending += MemberRemote_MessageSending;

            await instance.BasicSendOrReceive();

            // If data is never sent, test method will time out.
            await taskCompletionSource.Task;

            instance.MemberRemote.MessageSending -= MemberRemote_MessageSending;

            void MemberRemote_MessageSending(object? sender, RemoteMessageSendingEventArgs e)
            {
                if (e.Message is RemoteDataMessage dataMessage)
                {
                    Assert.AreEqual(nameof(DataProxyRemotingTestClass.BasicSendOrReceive), dataMessage.Token);
                    Assert.AreEqual(instance.MemberRemote.Id, dataMessage.MemberRemoteId);

                    taskCompletionSource.SetResult(null);
                }
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestPublishReceiveAsync()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            // Receiver with different member remote ID should not receive changes.
            var receiver2 = new DataProxyRemotingTestClass(RemotingMode.Client, "instance2");

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote, receiver2.MemberRemote);

            var expectedResult = Guid.NewGuid();

            // Tests by manually invoking both sender and listener 
            var receiverTask = receiver.BasicSendOrReceive();
            var receiver2Task = receiver2.BasicSendOrReceive();
            var senderTask = sender.BasicSendOrReceive(expectedResult);

            var results = await Task.WhenAll(receiverTask, senderTask);

            Assert.IsFalse(receiver2Task.IsCompleted, "Receiver with different member remote ID should not receive changes.");
            Assert.IsNotNull(results);
            Assert.AreEqual(expectedResult, results[0]);
            Assert.AreEqual(expectedResult, results[1]);
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestMultiPublishMultiReceiveAsync()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            // Receiver with different member remote ID should not receive changes.
            var receiver2 = new DataProxyRemotingTestClass(RemotingMode.Client, "instance2");

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote, receiver2.MemberRemote);

            var expectedResults = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }.ToList();

            // Tests by manually invoking both sender and listener 
            var receiverTask = receiver.MultiSendOrReceive(TimeSpan.FromMilliseconds(1000), expectedResults);
            var receiver2Task = receiver2.MultiSendOrReceive(TimeSpan.FromMilliseconds(1000), expectedResults);
            var senderTask = sender.MultiSendOrReceive(TimeSpan.FromMilliseconds(1000), expectedResults);

            var results = await Task.WhenAll(receiverTask, senderTask, receiver2Task);

            Assert.IsNotNull(results);
            Assert.AreNotEqual(0, expectedResults.Count);
            CollectionAssert.AreEqual(expectedResults, results[0]);
            CollectionAssert.AreEqual(expectedResults, results[1]);
            Assert.AreEqual(0, results[2].Count, "Receiver with different member remote ID should not receive changes.");
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestPublishReceiveRemoteMethod()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            var expectedResult = Guid.NewGuid();

            // Tests by manually invoking listener, and expecting RemoteMethod to call the sender, making it publish return data.
            var result = await receiver.RemoteSendOrReceive(expectedResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestMultiPublishReceiveRemoteMethod()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            var expectedResults = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }.ToList();

            // Tests by manually invoking listener, and expecting RemoteMethod to call the sender, making it publish return data.
            var result = await receiver.RemoteMultiSendOrReceive(TimeSpan.FromMilliseconds(1000), expectedResults);

            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(expectedResults, result);
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestPublishReceiveRemoteMethod_Null()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            // Tests by manually invoking listener, and expecting RemoteMethod to call the sender, making it publish return data.
            var result = await receiver.SendReceiveNull();

            Assert.IsNull(result);
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestPublishReceiveRemoteMethod_Int()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            // Tests by manually invoking listener, and expecting RemoteMethod to call the sender, making it publish return data.
            var result = await receiver.SendReceiveInt(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestPublishReceiveRemoteMethod_Object()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            var expectedValue = new DataProxyRemotingTestClass_Data()
            {
                Data = new DataProxyRemotingTestClass_Data()
                {
                    Time = DateTime.Now,
                },
            };

            // Tests by manually invoking listener, and expecting RemoteMethod to call the sender, making it publish return data.
            var result = await receiver.SendReceiveObj(expectedValue);

            Assert.IsNotNull(result);
            Helpers.SmartAssertEqual(expectedValue, expectedValue.GetType(), result, result.GetType());
        }

        private void WeaveLoopbackHandlers(params MemberRemote[] memberRemotes)
        {
            var handlers = new List<LoopbackMockMessageHandler>();

            foreach (var item in memberRemotes)
            {
                handlers.Add(item.MessageHandler.Cast<LoopbackMockMessageHandler>());
            }

            foreach (var handler in handlers)
                handler.LoopbackListeners.AddRange(handlers);
        }
    }

    [RemoteOptions(RemotingDirection.Bidirectional)]
    public class DataProxyRemotingTestClass
    {
        public MemberRemote MemberRemote { get; }

        public DataProxyRemotingTestClass(RemotingMode mode, string id = "testId")
        {
            MemberRemote = new MemberRemote(this, id, new LoopbackMockMessageHandler(mode));
        }

        [RemoteMethod]
        public Task<Guid?> RemoteSendOrReceive(Guid? guid = null) => BasicSendOrReceive(guid);

        public Task<Guid?> BasicSendOrReceive(Guid? guid = null)
        {
            if (MemberRemote.Mode == RemotingMode.Client)
                return MemberRemote.ReceiveDataAsync<Guid?>(nameof(BasicSendOrReceive));

            if (MemberRemote.Mode == RemotingMode.Host)
                return MemberRemote.PublishDataAsync(nameof(BasicSendOrReceive), guid);

            throw new ArgumentOutOfRangeException();
        }

        [RemoteMethod]
        public Task<List<Guid>> RemoteMultiSendOrReceive(TimeSpan timespan, List<Guid> guid) => MultiSendOrReceive(timespan, guid);

        public async Task<List<Guid>> MultiSendOrReceive(TimeSpan timespan, List<Guid> guid)
        {
            if (MemberRemote.Mode == RemotingMode.Client)
                return await MemberRemote.ReceiveDataAsync<Guid>(nameof(MultiSendOrReceive), timespan);

            if (MemberRemote.Mode == RemotingMode.Host)
            {
                foreach (var item in guid)
                {
                    await Task.Delay(10);
                    await MemberRemote.PublishDataAsync(nameof(MultiSendOrReceive), item);
                }

                return guid;
            }

            throw new ArgumentOutOfRangeException();
        }

        [RemoteMethod]
        public Task<object?> SendReceiveNull()
        {
            if (MemberRemote.Mode == RemotingMode.Client)
                return MemberRemote.ReceiveDataAsync<object?>(nameof(SendReceiveNull));

            if (MemberRemote.Mode == RemotingMode.Host)
                return MemberRemote.PublishDataAsync<object?>(nameof(SendReceiveNull), null);

            throw new ArgumentOutOfRangeException();
        }

        [RemoteMethod]
        public Task<int?> SendReceiveInt(int val)
        {
            if (MemberRemote.Mode == RemotingMode.Client)
                return MemberRemote.ReceiveDataAsync<int?>(nameof(SendReceiveNull));

            if (MemberRemote.Mode == RemotingMode.Host)
                return MemberRemote.PublishDataAsync<int?>(nameof(SendReceiveNull), val);

            throw new ArgumentOutOfRangeException();
        }

        [RemoteMethod]
        public Task<DataProxyRemotingTestClass_Data?> SendReceiveObj(DataProxyRemotingTestClass_Data? val)
        {
            if (MemberRemote.Mode == RemotingMode.Client)
                return MemberRemote.ReceiveDataAsync<DataProxyRemotingTestClass_Data>(nameof(SendReceiveNull));

            if (MemberRemote.Mode == RemotingMode.Host)
                return MemberRemote.PublishDataAsync(nameof(SendReceiveNull), val);

            throw new ArgumentOutOfRangeException();
        }
    }

    public class DataProxyRemotingTestClass_Data
    {
        public int Number { get; }

        public DateTime Time { get; set; }

        public DataProxyRemotingTestClass_Data? Data { get; set; }
    }
}
