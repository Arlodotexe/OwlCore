using OwlCore.Remoting.Transfer.MessageConverters;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Services;

namespace OwlCore.Remoting.Transfer
{
    /// <summary>
    /// Prepares the data in a <see cref="IRemoteMessage"/> for generic data transfer.
    /// </summary>
    /// <remarks>
    /// Several default implementations are given in the <see cref="MessageConverters"/> namespace.
    /// Of these, the <see cref="NewtonsoftRemoteMessageConverter"/> is the most reliable and should be used by default, unless you know what you're doing.
    /// <para>
    /// When crafting your own <see cref="IRemoteMessageConverter"/>, keep in mind that unless you're extra careful to remote only primitive types, object instances being serialized may need to handle circular references.
    /// </para>
    /// </remarks>
    public interface IRemoteMessageConverter : IAsyncSerializer<byte[], IRemoteMessage>
    {
    }
}
