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
        [DataRow, DataRow, DataRow, DataRow, DataRow] // run it a few times
        [Timeout(1000)]
        public async Task TestPublishAndReceiveAsync()
        {
            var sender = new DataProxyRemotingTestClass(RemotingMode.Host);
            var receiver = new DataProxyRemotingTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            var results = await Task.WhenAll(sender.DoSomething(), receiver.DoSomething());

            Assert.IsNotNull(results);
            Assert.AreEqual(results[0], results[1]);
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

        public Task<Guid> DoSomething()
        {
            if (MemberRemote.Mode == RemotingMode.Client)
                return MemberRemote.ReceiveDataAsync<Guid>(nameof(DoSomething));

            if (MemberRemote.Mode == RemotingMode.Host)
                return MemberRemote.PublishDataAsync(nameof(DoSomething), Guid.NewGuid());

            throw new ArgumentOutOfRangeException();
        }
    }
}
