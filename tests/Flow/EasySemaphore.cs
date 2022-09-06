using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Extensions;
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
    }
}
