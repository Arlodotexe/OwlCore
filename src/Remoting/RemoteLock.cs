using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Remoting
{
    /// <summary>
    /// Extension methods for asynchronously locking entry of a thread until it is released remotely.
    /// </summary>
    public static class RemoteLock
    {
        private static readonly ConcurrentDictionary<string, Task<object?>> _remoteLockHandles = new ConcurrentDictionary<string, Task<object?>>();

        /// <summary>
        /// Locks entry until released remotely, scoped to the given <paramref name="memberRemote"/> and <paramref name="token"/>.
        /// </summary>
        /// <param name="memberRemote">The member remote used to receive the message.</param>
        /// <param name="token">A unique token used to identify this lock between remoting nodes.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. Value is the received data.</returns>
        public static Task RemoteWaitAsync(this MemberRemote memberRemote, string token, CancellationToken? cancellationToken = null)
        {
            return DataProxy.ReceiveDataAsync<object?>(memberRemote, CreateScopedToken(token), cancellationToken);
        }

        /// <summary>
        /// Remotely releases all locks that match the given <paramref name="token"/>.
        /// </summary>
        /// <param name="memberRemote">The member remote used to send the message.</param>
        /// <param name="token">A unique token used to identify the lock being released.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task RemoteReleaseAsync(this MemberRemote memberRemote, string token, CancellationToken? cancellationToken = null)
        {
            return DataProxy.PublishDataAsync<object?>(memberRemote, CreateScopedToken(token), null, cancellationToken);
        }

        /// <summary>
        /// Wraps around a given token and scopes it to the remote lock functionality and a specific member remote.
        /// </summary>
        /// <param name="token">The user-provided, local scoped token</param>
        /// <returns></returns>
        public static string CreateScopedToken(string token)
        {
            return $"{nameof(RemoteLock)}.{token}";
        }
    }
}
