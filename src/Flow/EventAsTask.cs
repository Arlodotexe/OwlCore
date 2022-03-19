using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OwlCore
{
    public static partial class Flow
    {
        /// <summary>
        /// Waits for an event to fire. If the <paramref name="cancellationToken"/> is cancelled, a default value is returned.
        /// </summary>
        /// <param name="subscribe">An action that is invoked to subscribe a listener delegate to an event handler.</param>
        /// <param name="unsubscribe">An action that is invoked to unsubscribe a listener delegate from an event handler.</param>
        /// <param name="cancellationToken">A token that can be used to cancel waiting for the event to fire.</param>
        /// <remarks>
        /// Example usage:
        /// <para/>
        /// <example>
        /// <c lang="csharp">
        /// var eventResult = Flow.EventAsTask&lt;TResult>(x => someClass.ThingHappened += x, x => someClass.ThingHappened -= x, cancellationToken); 
        /// </c>
        /// </example>
        /// </remarks>
        /// <exception cref="OperationCanceledException"/>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task<(object? Sender, TResult Result)?> EventAsTask<TResult>(Action<EventHandler<TResult>> subscribe, Action<EventHandler<TResult>> unsubscribe, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<(object? Sender, TResult Result)>();
            var unsubbed = false;

            cancellationToken.Register(() =>
            {
                if (!unsubbed)
                {
                    unsubscribe(EventHandler);
                    unsubbed = true;
                }

                if (!completionSource.Task.IsCompleted)
                    completionSource.SetCanceled();
            });

            subscribe(EventHandler);

            try
            {
                var result = await completionSource.Task;

                if (!unsubbed)
                {
                    unsubscribe(EventHandler);
                    unsubbed = true;
                }

                return result;
            }
            catch (TaskCanceledException)
            {
                if (!unsubbed)
                {
                    unsubscribe(EventHandler);
                    unsubbed = true;
                }

                return null;
            }

            void EventHandler(object sender, TResult eventArgs) => completionSource.SetResult((sender, eventArgs));
        }

        /// <summary>
        /// Waits for an event to fire. If the <paramref name="cancellationToken"/> is cancelled, a default value is returned.
        /// </summary>
        /// <param name="subscribe">An action that is invoked to subscribe a listener delegate to an event handler.</param>
        /// <param name="unsubscribe">An action that is invoked to unsubscribe a listener delegate from an event handler.</param>
        /// <param name="cancellationToken">A token that can be used to cancel waiting for the event to fire.</param>
        /// <remarks>
        /// Example usage:
        /// <para/>
        /// <example>
        /// <c lang="csharp">
        /// var eventResult = Flow.EventAsTask&lt;TResult>(x => someClass.ThingHappened += x, x => someClass.ThingHappened -= x, cancellationToken); 
        /// </c>
        /// </example>
        /// </remarks>
        /// <exception cref="OperationCanceledException"/>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task<(object? Sender, EventArgs Result)?> EventAsTask(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<(object? Sender, EventArgs Result)>();
            var unsubbed = false;

            cancellationToken.Register(() =>
            {
                if (!unsubbed)
                {
                    unsubscribe(EventHandler);
                    unsubbed = true;
                }

                if (!completionSource.Task.IsCompleted)
                    completionSource.SetCanceled();
            });

            subscribe(EventHandler);

            try
            {
                var result = await completionSource.Task;
                if (!unsubbed)
                {
                    unsubscribe(EventHandler);
                    unsubbed = true;
                }
                return result;
            }
            catch (TaskCanceledException)
            {
                if (!unsubbed)
                {
                    unsubscribe(EventHandler);
                    unsubbed = true;
                }
                return null;
            }

            void EventHandler(object sender, EventArgs eventArgs) => completionSource.SetResult((sender, eventArgs));
        }

        /// <summary>
        /// Waits for an event to fire. If the event doesn't fire within the given timeout, a default value is returned.
        /// </summary>
        /// <param name="subscribe">An action that is invoked to subscribe a listener delegate to an event handler.</param>
        /// <param name="unsubscribe">An action that is invoked to unsubscribe a listener delegate from an event handler.</param>
        /// <param name="timeToWait">A token that can be used to cancel waiting for the event to fire.</param>
        /// <remarks>
        /// Example usage:
        /// <para/>
        /// <example>
        /// <c lang="csharp">
        /// var eventResult = Flow.EventAsTask&lt;TResult>(x => someClass.ThingHappened += x, x => someClass.ThingHappened -= x, TimeSpan.FromSeconds(2)); 
        /// </c>
        /// </example>
        /// </remarks>
        /// <exception cref="OperationCanceledException"/>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static Task<(object? Sender, TResult Result)?> EventAsTask<TResult>(Action<EventHandler<TResult>> subscribe, Action<EventHandler<TResult>> unsubscribe, TimeSpan timeToWait)
        {
            var resultCancellationToken = new CancellationTokenSource();

            resultCancellationToken.CancelAfter(timeToWait);

            return EventAsTask(subscribe, unsubscribe, resultCancellationToken.Token);
        }

        /// <summary>
        /// Waits for an event to fire. If the event doesn't fire within the given timeout, a default value is returned.
        /// </summary>
        /// <param name="subscribe">An action that is invoked to subscribe a listener delegate to an event handler.</param>
        /// <param name="unsubscribe">An action that is invoked to unsubscribe a listener delegate from an event handler.</param>
        /// <param name="timeToWait">A token that can be used to cancel waiting for the event to fire.</param>
        /// <remarks>
        /// Example usage:
        /// <para/>
        /// <example>
        /// <c lang="csharp">
        /// var eventResult = Flow.EventAsTask&lt;TResult>(x => someClass.ThingHappened += x, x => someClass.ThingHappened -= x, TimeSpan.FromSeconds(2)); 
        /// </c>
        /// </example>
        /// </remarks>
        /// <exception cref="OperationCanceledException"/>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static Task<(object? Sender, EventArgs Result)?> EventAsTask(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe, TimeSpan timeToWait)
        {
            var resultCancellationToken = new CancellationTokenSource();

            resultCancellationToken.CancelAfter(timeToWait);

            return EventAsTask(subscribe, unsubscribe, resultCancellationToken.Token);
        }
    }
}