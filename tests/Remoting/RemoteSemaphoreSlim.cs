using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Remoting;
using OwlCore.Extensions;
using OwlCore.Tests.Remoting.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OwlCore.Tests.Remoting
{
    [TestClass]
    public class RemoteSemaphoreSlim
    {
        [DataRow(1, 0), DataRow(1, 1)]
        [DataRow(2, 0), DataRow(2, 1), DataRow(2, 2)]
        [DataRow(5, 0), DataRow(5, 1), DataRow(5, 2), DataRow(5, 5)]
        [DataRow(10, 0), DataRow(10, 1), DataRow(10, 2), DataRow(10, 5), DataRow(10, 10)]
        [TestMethod, Timeout(5000)]
        public async Task WaitAsync(int initialCount, int entryCount)
        {
            Assert.AreNotEqual(0, initialCount);
            var senderMsgHandler = new LoopbackMockMessageHandler(RemotingMode.Host);
            var receiverMsgHandler = new LoopbackMockMessageHandler(RemotingMode.Client);
            var id = $"{nameof(WaitAsync)}.{initialCount}";
            var timesReceiverEntered = 0;

            WeaveLoopbackHandlers(senderMsgHandler, receiverMsgHandler);

            var senderSemaphore = new OwlCore.Remoting.RemoteSemaphoreSlim(id, initialCount, senderMsgHandler);
            var receiverSemaphore = new OwlCore.Remoting.RemoteSemaphoreSlim(id, initialCount, receiverMsgHandler);

            // Ensure initial states are in sync.
            Assert.AreEqual(senderSemaphore.CurrentCount, receiverSemaphore.CurrentCount);

            var receiverEnteredTaskCompletionSource = new TaskCompletionSource();
            receiverSemaphore.SemaphoreEntered += OnReceiverEntered;

            // Enter sender
            for (int i = 0; i < entryCount; i++)
                await senderSemaphore.WaitAsync();

            // Ensure sender entered
            Assert.AreEqual(initialCount - entryCount, senderSemaphore.CurrentCount);

            if (entryCount == 0)
            {
                // Ensure the receiver was not entered.
                await Task.Delay(250);
                Assert.AreEqual(senderSemaphore.CurrentCount, receiverSemaphore.CurrentCount);
                return;
            }

            // Wait for receiver entry
            await receiverEnteredTaskCompletionSource.Task;

            // Ensure receiver entry is in sync with sender.
            Assert.AreEqual(senderSemaphore.CurrentCount, receiverSemaphore.CurrentCount);

            receiverSemaphore.SemaphoreEntered -= OnReceiverEntered;

            void OnReceiverEntered(object? sender, EventArgs e)
            {
                Assert.IsFalse(receiverEnteredTaskCompletionSource.Task.IsCompleted);
                timesReceiverEntered++;

                if (timesReceiverEntered == entryCount)
                    receiverEnteredTaskCompletionSource.SetResult();
            }
        }

        [DataRow(1, 0), DataRow(1, 1)]
        [DataRow(2, 0), DataRow(2, 1), DataRow(2, 2)]
        [DataRow(5, 0), DataRow(5, 1), DataRow(5, 2), DataRow(5, 5)]
        [DataRow(10, 0), DataRow(10, 1), DataRow(10, 2), DataRow(10, 5), DataRow(10, 10)]
        [TestMethod, Timeout(5000)]
        public async Task WaitAndReleaseAsync(int initialCount, int entryCount)
        {
            Assert.AreNotEqual(0, initialCount);
            var senderMsgHandler = new LoopbackMockMessageHandler(RemotingMode.Host);
            var receiverMsgHandler = new LoopbackMockMessageHandler(RemotingMode.Client);
            var id = $"{nameof(WaitAndReleaseAsync)}.{initialCount}";
            var timesReceiverEntered = 0;
            var timesReceiverReleased = 0;
            var timesSenderReleased = 0;

            WeaveLoopbackHandlers(senderMsgHandler, receiverMsgHandler);

            var senderSemaphore = new OwlCore.Remoting.RemoteSemaphoreSlim(id, initialCount, senderMsgHandler);
            var receiverSemaphore = new OwlCore.Remoting.RemoteSemaphoreSlim(id, initialCount, receiverMsgHandler);

            // Ensure initial states are in sync.
            Assert.AreEqual(senderSemaphore.CurrentCount, receiverSemaphore.CurrentCount);

            var receiverEnteredTaskCompletionSource = new TaskCompletionSource();
            receiverSemaphore.SemaphoreEntered += OnReceiverEntered;

            var receiverReleasedTaskCompletionSource = new TaskCompletionSource();
            receiverSemaphore.SemaphoreReleased += OnReceiverReleased;

            var senderReleasedTaskCompletionSource = new TaskCompletionSource();
            receiverSemaphore.SemaphoreReleased += OnSenderReleased;

            // Enter sender
            for (int i = 0; i < entryCount; i++)
                await senderSemaphore.WaitAsync();

            // Ensure sender entered
            Assert.AreEqual(initialCount - entryCount, senderSemaphore.CurrentCount);

            if (entryCount == 0)
            {
                // Ensure the receiver was not entered.
                await Task.Delay(250);
                Assert.AreEqual(senderSemaphore.CurrentCount, receiverSemaphore.CurrentCount);
                return;
            }

            await receiverEnteredTaskCompletionSource.Task;

            // Ensure receiver entry is in sync with sender.
            Assert.AreEqual(senderSemaphore.CurrentCount, receiverSemaphore.CurrentCount);

            // Release sender
            for (int i = 0; i < entryCount; i++)
                senderSemaphore.Release();

            // Wait for sender and receiver to be completely released.
            await Task.WhenAll(receiverReleasedTaskCompletionSource.Task, senderReleasedTaskCompletionSource.Task);

            // Ensure both completely released.
            Assert.AreEqual(initialCount, senderSemaphore.CurrentCount);
            Assert.AreEqual(initialCount, receiverSemaphore.CurrentCount);

            void OnReceiverEntered(object? sender, EventArgs e)
            {
                Assert.IsFalse(receiverEnteredTaskCompletionSource.Task.IsCompleted);

                if (++timesReceiverEntered == entryCount)
                    receiverEnteredTaskCompletionSource.SetResult();
            }

            void OnReceiverReleased(object? sender, EventArgs e)
            {
                Assert.IsFalse(receiverReleasedTaskCompletionSource.Task.IsCompleted);

                if (++timesReceiverReleased == entryCount)
                    receiverReleasedTaskCompletionSource.SetResult();
            }

            void OnSenderReleased(object? sender, EventArgs e)
            {
                Assert.IsFalse(senderReleasedTaskCompletionSource.Task.IsCompleted);

                if (++timesSenderReleased == entryCount)
                    senderReleasedTaskCompletionSource.SetResult();
            }
        }

        private void WeaveLoopbackHandlers(params LoopbackMockMessageHandler[] messageHandlers)
        {
            foreach (var handler in messageHandlers)
                handler.LoopbackListeners.AddRange(messageHandlers.Except(handler.IntoList()));
        }
    }
}
