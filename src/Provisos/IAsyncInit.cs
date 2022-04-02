using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Provisos
{
    /// <summary>
    /// Implementors of this interface indicate that asynchronous initialization can be performed.
    /// </summary>
    public interface IAsyncInit
    {
        /// <summary>
        /// Runs generic asynchronous initialization that would have otherwise been performed in the constructor.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indicated whether or not the instance has been initialized.
        /// </summary>
        bool IsInitialized { get; }
    }
}