using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Extensions;
using OwlCore.Remoting;
using OwlCore.Remoting.Transfer;
using OwlCore.Tests.Remoting.Transfer;

namespace OwlCore.Tests.Flow
{
    [TestClass]
    public class EasySemaphoreTests
    {
        [TestMethod]
        [DataRow(1, 1), DataRow(5, 1), DataRow(5, 3)]
        [Timeout(1000)]
        public async Task SemaphoreSlim(int maxHandleCount, int initialHandleCount)
        {
            using var semaphore = new SemaphoreSlim(initialHandleCount, maxHandleCount);

            using (await OwlCore.Flow.EasySemaphore(semaphore))
            {
                Assert.AreEqual(semaphore.CurrentCount, initialHandleCount - 1);
                await Task.Delay(15);
                Assert.AreEqual(semaphore.CurrentCount, initialHandleCount - 1);
            }

            Assert.AreEqual(semaphore.CurrentCount, initialHandleCount);
        }

        [TestMethod]
        [DataRow(1, 1), DataRow(5, 1), DataRow(5, 3)]
        [Timeout(1000)]
        public async Task RemoteSemaphoreSlim(int maxHandleCount, int initialHandleCount)
        {
            var sender = new EasyRemoteSemaphoreSlimTestClass(RemotingMode.Host, maxHandleCount, initialHandleCount);
            var receiver = new EasyRemoteSemaphoreSlimTestClass(RemotingMode.Client, maxHandleCount, initialHandleCount);

            WeaveLoopbackHandlers(sender.MessageHandler, receiver.MessageHandler);

            var receiverEnteredTask = OwlCore.Flow.EventAsTask(x => receiver.RemoteSemaphore.SemaphoreEntered += x, x => receiver.RemoteSemaphore.SemaphoreEntered -= x, CancellationToken.None);
            var receiverReleasedTask = OwlCore.Flow.EventAsTask(x => receiver.RemoteSemaphore.SemaphoreReleased += x, x => receiver.RemoteSemaphore.SemaphoreReleased -= x, CancellationToken.None);

            var senderEnteredTask = OwlCore.Flow.EventAsTask(x => sender.RemoteSemaphore.SemaphoreEntered += x, x => sender.RemoteSemaphore.SemaphoreEntered -= x, CancellationToken.None);
            var senderReleasedTask = OwlCore.Flow.EventAsTask(x => sender.RemoteSemaphore.SemaphoreReleased += x, x => sender.RemoteSemaphore.SemaphoreReleased -= x, CancellationToken.None);

            _ = receiverEnteredTask.ContinueWith(x =>
            {
                // Ensure receiver entered semaphore after sender has entered.
                Assert.IsTrue(senderEnteredTask.IsCompleted);

                // Ensure receiver entered semaphore after sender has released.
                Assert.IsTrue(senderReleasedTask.IsCompleted);
            });

            _ = receiverReleasedTask.ContinueWith(x =>
            {
                // Ensure receiver released semaphore after sender has entered.
                Assert.IsTrue(senderEnteredTask.IsCompleted);

                // Ensure receiver released semaphore after sender has released.
                Assert.IsTrue(senderReleasedTask.IsCompleted);
            });

            Assert.IsFalse(receiverEnteredTask.IsCompleted, "Receiver entered prematurely.");
            Assert.IsFalse(receiverReleasedTask.IsCompleted, "Receiver released prematurely.");

            Assert.IsFalse(senderEnteredTask.IsCompleted, "Sender entered prematurely.");
            Assert.IsFalse(senderReleasedTask.IsCompleted, "Sender released prematurely.");

            await sender.RemoteMethod(TimeSpan.FromMilliseconds(250));

            await senderEnteredTask;
            await senderReleasedTask;

            await receiverEnteredTask;
            await receiverReleasedTask;
        }

        [RemoteOptions(RemotingDirection.Bidirectional)]
        public class EasyRemoteSemaphoreSlimTestClass
        {
            private MemberRemote _memberRemote;

            public EasyRemoteSemaphoreSlimTestClass(RemotingMode mode, int maxHandleCount, int initialHandleCount)
            {
                MessageHandler = new LoopbackMockMessageHandler(mode);
                _memberRemote = new MemberRemote(this, $"{nameof(EasyRemoteSemaphoreSlimTestClass)}{maxHandleCount}{initialHandleCount}", MessageHandler);

                RemoteSemaphore = new RemoteSemaphoreSlim($"{_memberRemote.Id}.Semaphore", initialHandleCount, maxHandleCount, MessageHandler);
            }

            public LoopbackMockMessageHandler MessageHandler { get; set; }

            public RemoteSemaphoreSlim RemoteSemaphore { get; }

            [RemoteMethod]
            public Task RemoteMethod(TimeSpan waitTime) => Task.Run(async () =>
            {
                using (await OwlCore.Flow.EasySemaphore(RemoteSemaphore))
                {
                    await Task.Delay(waitTime);
                }
            });
        }

        private void WeaveLoopbackHandlers(params LoopbackMockMessageHandler[] messageHandlers)
        {
            foreach (var handler in messageHandlers)
                handler.LoopbackListeners.AddRange(messageHandlers.Except(handler.IntoList()));
        }
    }
}
