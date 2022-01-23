using OwlCore.Remoting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore
{
    public static partial class Flow
    {
        /// <summary>
        /// Provides syntactic sugar for releasing a <see cref="SemaphoreSlim"/> when execution leaves a <c>using</c> statement.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. When this task completes, the semaphore has entered. Value is a disposable wrapper around the semaphore that calls <see cref="SemaphoreSlim.Release()"/> when disposed.</returns>
        public static async Task<DisposeToReleaseSemaphoreWrapper> EasySemaphore(SemaphoreSlim semaphore)
        {
            var wrapper = new DisposeToReleaseSemaphoreWrapper(semaphore);
            await semaphore.WaitAsync();
            return wrapper;
        }

        /// <summary>
        /// A wrapper that disposes the given semaphore when disposed. Used for <see cref="Flow.EasySemaphore(SemaphoreSlim)"/>.
        /// </summary>
        public class DisposeToReleaseSemaphoreWrapper : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            /// <summary>
            /// Creates a new instance of <see cref="DisposeToReleaseSemaphoreWrapper"/>.
            /// </summary>
            /// <param name="semaphore">The semaphore to wrap around and release when disposed.</param>
            public DisposeToReleaseSemaphoreWrapper(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            /// <summary>
            /// Called to release the semaphore.
            /// </summary>
            public void Dispose() => _semaphore.Release();
        }

        /// <summary>
        /// Asynchronously waits to enter the <see cref="RemoteSemaphoreSlim"/>. 
        /// If the semaphore has been entered remotely, it will wait until <see cref="RemoteSemaphoreSlim.Release"/> is called (locally or remotely).
        /// </summary>
        /// <remarks>
        /// Calling this method will also enter the semaphore on all remote instances of <see cref="RemoteSemaphoreSlim"/>, keeping the semaphore in sync.
        /// <para/>
        /// This task may complete before all other nodes have also entered the semaphore.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. When this task completes, the semaphore has entered. Value is a disposable wrapper around the semaphore that calls <see cref="SemaphoreSlim.Release()"/> when disposed.</returns>
        public static async Task<DisposeToReleaseRemoteSemaphoreWrapper> EasySemaphore(RemoteSemaphoreSlim semaphore)
        {
            var wrapper = new DisposeToReleaseRemoteSemaphoreWrapper(semaphore);
            await semaphore.WaitAsync();
            return wrapper;
        }

        /// <summary>
        /// A wrapper that disposes the given semaphore when disposed. Used for <see cref="Flow.EasySemaphore(SemaphoreSlim)"/>.
        /// </summary>
        public class DisposeToReleaseRemoteSemaphoreWrapper : IDisposable
        {
            private readonly RemoteSemaphoreSlim _semaphore;

            /// <summary>
            /// Creates a new instance of <see cref="DisposeToReleaseSemaphoreWrapper"/>.
            /// </summary>
            /// <param name="semaphore">The semaphore to wrap around and release when disposed.</param>
            public DisposeToReleaseRemoteSemaphoreWrapper(RemoteSemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            /// <summary>
            /// Called to release the semaphore.
            /// </summary>
            public void Dispose() => _semaphore.Release();
        }
    }
}
