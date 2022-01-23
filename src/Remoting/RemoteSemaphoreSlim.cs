using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Remoting.Transfer;

namespace OwlCore.Remoting
{
    /// <summary>
    /// A remote-capable <see cref="SemaphoreSlim"/>.
    /// </summary>
    /// <remarks>
    /// Due to the pubsub nature of OwlCore.Remoting, calling <see cref="Release"/> releases the semaphore on ALL listening nodes.
    /// Not taking this into account may cause undesired behavior.
    /// </remarks>
    [RemoteOptions(RemotingDirection.Bidirectional)]
    public class RemoteSemaphoreSlim : IDisposable
    {
        private readonly MemberRemote _memberRemote;
        private SemaphoreSlim _semaphore;

        /// <summary>
        /// Creates a new instance of <see cref="RemoteSemaphoreSlim"/>.
        /// </summary>
        /// <param name="id">A unique identifier for this <see cref="RemoteSemaphoreSlim"/>, consistent across all nodes.</param>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        /// <param name="messageHandler">The message handler to use when communicating remotely.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is less than 0.</exception>  
        public RemoteSemaphoreSlim(string id, int initialCount, IRemoteMessageHandler messageHandler)
            : this(id, initialCount, initialCount, messageHandler)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RemoteSemaphoreSlim"/>.
        /// </summary>
        /// <param name="messageHandler">The message handler to use when communicating remotely.</param>
        /// <param name="id">A unique identifier for this <see cref="RemoteSemaphoreSlim"/>, consistent across all nodes.</param>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is less than 0, or <paramref name="initialCount"/> is greater than <paramref name="maxCount"/>, or <paramref name="maxCount"/> is equal to or less than 0.</exception>  
        public RemoteSemaphoreSlim(string id, int initialCount, int maxCount, IRemoteMessageHandler messageHandler)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
            _memberRemote = new MemberRemote(this, $"{id}.{nameof(RemoteSemaphoreSlim)}", messageHandler);
        }

        /// <summary>
        /// Gets the number of available threads that can enter the <see cref="RemoteSemaphoreSlim"/> object.
        /// </summary>
        public int CurrentCount => _semaphore.CurrentCount;

        /// <summary>
        /// Gets the <see cref="MemberRemote"/> used to relay messages for this semaphore.
        /// </summary>
        public MemberRemote MemberRemote => _memberRemote;

        /// <summary>
        /// Asynchronously waits to enter the <see cref="RemoteSemaphoreSlim"/>. 
        /// If the semaphore has been entered remotely, it will wait until <see cref="Release"/> is called (locally or remotely).
        /// </summary>
        /// <remarks>
        /// Calling this method will also enter the semaphore on all remote instances of <see cref="RemoteSemaphoreSlim"/>, keeping the semaphore in sync.
        /// <para/>
        /// This task may complete before all other nodes have also entered the semaphore.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that completes when the semaphore has been entered.</returns>
        [RemoteMethod]
        public Task WaitAsync() => Task.Run(async () =>
        {
            await _semaphore.WaitAsync();

            SemaphoreEntered?.Invoke(this, EventArgs.Empty);
        });

        /// <summary>
        /// Releases the semaphore locally and remotely in a fire-and-forget manner.
        /// </summary>
        /// <remarks>
        /// Due to the pubsub nature of OwlCore.Remoting, calling <see cref="Release"/> releases the semaphore on ALL listening nodes.
        /// Not taking this into account may cause undesired behavior.
        /// </remarks>
        [RemoteMethod]
        public void Release()
        {
            _semaphore.Release();

            SemaphoreReleased?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the semaphore is entered.
        /// </summary>
        internal event EventHandler? SemaphoreEntered;

        /// <summary>
        /// Raised when the semaphore is released.
        /// </summary>
        internal event EventHandler? SemaphoreReleased;

        /// <summary>
        /// Disposes of internal resources (<see cref="SemaphoreSlim"/>, <see cref="Remoting.MemberRemote"/>, etc.).
        /// </summary>
        /// <remarks>
        /// Will not be disposed remotely. Each node must dispose their own instance.
        /// </remarks>
        public void Dispose()
        {
            MemberRemote.Dispose();
            _semaphore.Dispose();
        }
    }
}
