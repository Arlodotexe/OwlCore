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
    public class RemoteLock
    {
        [TestMethod]
        [Timeout(1000)]
        public async Task TestReleaseAsync()
        {
            var instance = new RemoteLockTestClass(RemotingMode.Host);
            var taskCompletionSource = new TaskCompletionSource<object?>();

            WeaveLoopbackHandlers(instance.MemberRemote);

            instance.MemberRemote.MessageSending += MemberRemote_MessageSending;

            _ = instance.DoSomething();

            // If data is never sent, test method will time out.
            await taskCompletionSource.Task;

            instance.MemberRemote.MessageSending -= MemberRemote_MessageSending;

            void MemberRemote_MessageSending(object? sender, RemoteMessageSendingEventArgs e)
            {
                if (e.Message is RemoteDataMessage dataMessage)
                {
                    var scopedToken = OwlCore.Remoting.RemoteLock.CreateScopedToken(nameof(instance.DoSomething));

                    Assert.AreEqual(scopedToken, dataMessage.Token);
                    Assert.AreEqual(instance.MemberRemote.Id, dataMessage.MemberRemoteId);

                    taskCompletionSource.SetResult(null);
                }
            }
        }

        [TestMethod]
        [Timeout(1000)]
        public async Task TestLockReleaseAsync()
        {
            var sender = new RemoteLockTestClass(RemotingMode.Host);
            var receiver = new RemoteLockTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            // Tests by manually invoking both sender and listener 
            var receiverTask = receiver.DoSomething();
            
            await sender.DoSomething();
            await receiverTask;
        }

        [TestMethod]
        [Timeout(1000)]
        public async Task TestLockReleaseWithRemoteMethodAsync()
        {
            var sender = new RemoteLockTestClass(RemotingMode.Host);
            var receiver = new RemoteLockTestClass(RemotingMode.Client);

            WeaveLoopbackHandlers(sender.MemberRemote, receiver.MemberRemote);

            // Tests by manually invoking listener, and expecting RemoteMethod to call the sender, making it unlock remotely.
            await receiver.DoSomethingRemote();
        }


        private void WeaveLoopbackHandlers(params MemberRemote[] memberRemotes)
        {
            var handlers = new List<LoopbackMockMessageHandler>();

            foreach (var item in memberRemotes)
                handlers.Add(item.MessageHandler.Cast<LoopbackMockMessageHandler>());

            foreach (var handler in handlers)
                handler.LoopbackListeners.AddRange(handlers);
        }
    }

    [RemoteOptions(RemotingDirection.Bidirectional)]
    public class RemoteLockTestClass
    {
        public MemberRemote MemberRemote { get; }

        public RemoteLockTestClass(RemotingMode mode)
        {
            MemberRemote = new MemberRemote(this, "testId", new LoopbackMockMessageHandler(mode));
        }

        [RemoteMethod]
        public Task DoSomethingRemote() => DoSomething();

        public async Task DoSomething()
        {
            if (MemberRemote.Mode == RemotingMode.Client)
            {
                await MemberRemote.RemoteWaitAsync(nameof(DoSomething));
                return;
            }

            if (MemberRemote.Mode == RemotingMode.Host)
            {
                await Task.Delay(100);
                await MemberRemote.RemoteReleaseAsync(nameof(DoSomething));
                return;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
