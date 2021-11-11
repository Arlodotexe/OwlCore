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
            /// <param name="semaphore"></param>
            public DisposeToReleaseSemaphoreWrapper(SemaphoreSlim semaphore)
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
