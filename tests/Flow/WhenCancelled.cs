using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OwlCore.Tests.Flow
{
    [TestClass]
    public class WhenCancelledTests
    {
        [TestMethod, Timeout(1000)]
        public async Task SimpleCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource(100);

            await OwlCore.Flow.WhenCancelled(cancellationTokenSource.Token, CancellationToken.None);

            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
        }

        [TestMethod, Timeout(1000)]
        public async Task WithSelfCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var selfCancellationTokenSource = new CancellationTokenSource(100);

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => OwlCore.Flow.WhenCancelled(cancellationTokenSource.Token, selfCancellationTokenSource.Token));

            Assert.IsFalse(cancellationTokenSource.IsCancellationRequested);
            Assert.IsTrue(selfCancellationTokenSource.IsCancellationRequested);
        }
    }
}
