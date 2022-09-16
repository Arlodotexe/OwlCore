using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

// ReSharper disable once CheckNamespace
namespace OwlCore
{
    /// <summary>
    /// Helpers related to Threading.
    /// </summary>
    public static partial class Threading
    {
        /// <summary>
        /// Sets the <see cref="SynchronizationContext"/> for your primary thread.
        /// </summary>
        /// <param name="context">Your primary synchronization context. Usually the context used in the project entry point.</param>
        public static void SetPrimarySynchronizationContext(SynchronizationContext context)
        {
            if (!(PrimarySyncContext is null))
                ThrowHelper.ThrowInvalidOperationException($"{nameof(SetPrimarySynchronizationContext)} cannot be used more than once.");

            PrimarySyncContext = context;
        }

        /// <summary>
        /// Sets a handler to use when invoking <see cref="OnPrimaryThread(Action)"/>.
        /// </summary>
        /// <param name="handler">A callback that invokes an action on your primary thread.</param>
        public static void SetPrimaryThreadInvokeHandler(Func<Action, Task> handler)
        {
            if (!(PrimaryThreadInvokeHandler is null))
                ThrowHelper.ThrowInvalidOperationException($"{nameof(SetPrimaryThreadInvokeHandler)} cannot be used more than once.");

            PrimaryThreadInvokeHandler = handler;
        }

        /// <summary>
        /// Invokes a callback on the primary thread.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The value returned by the <paramref name="callback"/></returns>
        public static async Task OnPrimaryThread(Action callback)
        {
            Guard.IsNotNull(PrimaryThreadInvokeHandler, nameof(PrimaryThreadInvokeHandler));

            await PrimaryThreadInvokeHandler(callback);
        }

        private static Func<Action, Task>? PrimaryThreadInvokeHandler;
    }
}