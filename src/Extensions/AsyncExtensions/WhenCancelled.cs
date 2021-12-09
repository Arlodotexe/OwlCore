using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OwlCore.Extensions
{
    public static partial class AsyncExtensions
    {
        /// <inheritdoc cref="Flow.WhenCancelled(CancellationToken, CancellationToken)"/>
        public static Task WhenCancelled(this CancellationToken cancellationToken, CancellationToken selfCancellationToken)
        {
            return Flow.WhenCancelled(cancellationToken, selfCancellationToken);
        }
    }
}