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
        [Timeout(1000)]
        public async Task TestPublishAsync()
        {
            var instance = new DataProxyRemotingTestClass(RemotingMode.Host);
            var taskCompletionSource = new TaskCompletionSource<object?>();

            WeaveLoopbackHandlers(instance.MemberRemote);

            instance.MemberRemote.MessageSending += MemberRemote_MessageSending;

            await instance.DoSomething();

            // If data is never sent, test method will time out.
            await taskCompletionSource.Task;

            instance.MemberRemote.MessageSending -= MemberRemote_MessageSending;

            void MemberRemote_MessageSending(object? sender, RemoteMessageSendingEventArgs e)
            {
                if (e.Message is RemoteDataMessage dataMessage)
                {
                    Assert.AreEqual(nameof(DataProxyRemotingTestClass.DoSomething), dataMessage.Token);
                    Assert.AreEqual(instance.MemberRemote.Id, dataMessage.MemberRemoteId);

                    taskCompletionSource.SetResult(null);
                }
            }
        }

        [TestMethod]
        [Timeout(1000)]
        public async Task TestPublishReceiveAsync()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            var expectedResult = Guid.NewGuid();

            // Tests by manually invoking both sender and listener 
            var results = await Task.WhenAll(sender.DoSomething(expectedResult), receiver.DoSomething());

            Assert.IsNotNull(results);
            Assert.AreEqual(expectedResult, results[0]);
            Assert.AreEqual(expectedResult, results[1]);
        }

        [TestMethod]
        [Timeout(1000)]
        public async Task TestPublishReceiveWithRemoteMethodAsync()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            var expectedResult = Guid.NewGuid();

            // Tests by manually invoking listener, and expecting RemoteMethod to call the sender, making it publish return data.
            var result = await receiver.DoSomethingRemote(expectedResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, result);
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

        public DataProxyRemotingTestClass(RemotingMode mode)
        {
            MemberRemote = new MemberRemote(this, "testId", new LoopbackMockMessageHandler(mode));
        }

        [RemoteMethod]
        public Task<Guid?> DoSomethingRemote(Guid? guid = null) => DoSomething(guid);

        public Task<Guid?> DoSomething(Guid? guid = null)
        {
            if (MemberRemote.Mode == RemotingMode.Client)
                return MemberRemote.ReceiveDataAsync<Guid?>(nameof(DoSomething));

            if (MemberRemote.Mode == RemotingMode.Host)
                return MemberRemote.PublishDataAsync(nameof(DoSomething), guid);

            throw new ArgumentOutOfRangeException();
        }
    }
}
