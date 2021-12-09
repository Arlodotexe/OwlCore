using System.Threading;
using System.Threading.Tasks;

namespace OwlCore
{
    public static partial class Flow
    {
        /// <summary>
        /// Awaits the cancellation of a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to watch for cancellation.</param>
        /// <param name="selfCancellationToken">When this token is cancelled, internal data is disposed and this task throw <see cref="TaskCanceledException"/>.</param>
        /// <returns>A <see cref="Task"/> that completes if the provided <paramref name="cancellationToken"/> is cancelled.</returns>
        /// <exception cref="TaskCanceledException"/>
        public static Task WhenCancelled(CancellationToken cancellationToken, CancellationToken selfCancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<object?>();

            var cancellationRegistration = cancellationToken.Register(() =>
            {
                taskCompletionSource.SetResult(null);
            });

            selfCancellationToken.Register(() =>
            {
                taskCompletionSource.SetCanceled();
                cancellationRegistration.Dispose();
            });

            return taskCompletionSource.Task;
        }
    }
}
