using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OwlCore.Extensions
{
    public static partial class AsyncExtensions
    {
        /// <summary>
        /// Posts to a <see cref="SynchronizationContext"/> asynchronously and waits for the task to complete.
        /// </summary>
        public static Task PostAsync(this SynchronizationContext syncContext, Func<Task> callback)
        {
            var taskCompletionSource = new TaskCompletionSource<object?>();

            syncContext.Post(async _ =>
            {
                await callback();
                taskCompletionSource.SetResult(null);
            }, null);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Posts to a <see cref="SynchronizationContext"/> asynchronously and waits for the task to complete.
        /// </summary>
        public static Task<TResult> PostAsync<TResult>(this SynchronizationContext syncContext, Func<Task<TResult>> callback)
        {
            var taskCompletionSource = new TaskCompletionSource<TResult>();

            syncContext.Post(async _ =>
            {
                var result = await callback();
                taskCompletionSource.SetResult(result);
            }, null);

            return taskCompletionSource.Task;
        }
    }
}