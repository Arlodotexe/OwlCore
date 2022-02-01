using System;
using System.Collections.Generic;

namespace OwlCore.ComponentModel
{
    /// <summary>
    /// Builds a proxied chain of <see cref="IDelegatable{T}"/> for each added link in this collection.
    /// </summary>
    /// <typeparam name="TChainLink">A link in the chain. Implements both <typeparamref name="TInner"/> and <see cref="IDelegatable{TInner}"/>.</typeparam>
    /// <typeparam name="TInner">The inner type that each <typeparamref name="TChainLink"/> is delegated to.</typeparam>
    public class ChainedProxyBuilder<TChainLink, TInner> : List<Func<TInner, TChainLink>>
        where TInner : class
        where TChainLink : class, TInner, IDelegatable<TInner>
    {
        /// <summary>
        /// Builds the items in this collection into a proxied chain, where each item might delegate member access to the next item.
        /// </summary>
        /// <param name="finalProxy">The innermost implementation that is called if all items in the chain call their provided <see cref="IDelegatable{T}.Inner"/>.</param>
        /// <returns>The first <typeparamref name="TChainLink"/>, with the next item provided during construction for proxying, which also had the next item passed into it, and so on.</returns>
        public TInner Execute(TInner finalProxy)
        {
            var current = finalProxy;

            for (int i = Count - 1; i >= 0; i--)
                current = this[i](current);

            return current;
        }
    }

}
