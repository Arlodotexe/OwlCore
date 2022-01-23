using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Tests.Extensions
{
    [TestClass]
    public class AsyncExtensions
    {
        [TestMethod, Timeout(1000)]
        public async Task WhenCancelled_SimpleCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource(100);

            await cancellationTokenSource.Token.WhenCancelled(CancellationToken.None);

            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
        }

        [TestMethod, Timeout(1000)]
        public async Task WhenCancelled_WithSelfCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var selfCancellationTokenSource = new CancellationTokenSource(100);

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => cancellationTokenSource.Token.WhenCancelled(selfCancellationTokenSource.Token));

            Assert.IsFalse(cancellationTokenSource.IsCancellationRequested);
            Assert.IsTrue(selfCancellationTokenSource.IsCancellationRequested);
        }
    }
}
