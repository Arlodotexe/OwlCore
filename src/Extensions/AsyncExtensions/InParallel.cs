using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OwlCore.Extensions
{
    /// <summary>
    /// Async-related extension methods.
    /// </summary>
    public static partial class AsyncExtensions
    {
        /// <summary>
        /// Runs a specific task in parallel from a list of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to operate on.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="func">Returns the task to run in parallel, given <typeparamref name="T"/>.</param>
        /// <returns>A task representing the completion of all tasks.</returns>
        public static async Task InParallel<T>(this IEnumerable<T> items, Func<T, Task> func)
        {
            var exceptions = new List<Exception>();

            // WhenAll returns after the first exception is thrown
            // So exceptions need to be captured manually.
            await Task.WhenAll(items.Select(x => CaptureThrownException(func(x), exceptions)));

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            static async Task CaptureThrownException(Task task, List<Exception> exceptions)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Runs a specific task in parallel from a list of <typeparamref name="T"/>, returning all the completed values.
        /// </summary>
        /// <typeparam name="T">The type to operate on.</typeparam>
        /// <typeparam name="T2">The return type.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="func">Returns the task to run in parallel, given <typeparamref name="T"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completion of all tasks. The result is an array of all the returned values.</returns>
        public static async Task<T2[]> InParallel<T, T2>(this IEnumerable<T> items, Func<T, Task<T2>> func)
        {
            var exceptions = new List<Exception>();

            // WhenAll returns after the first exception is thrown
            // So exceptions need to be captured manually.
            var res = await Task.WhenAll(items.Select(x => CaptureThrownException(func(x), exceptions)));

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            Guard.IsNotNull(res);

            return res.Where(x => x is not null).Cast<T2>().ToArray();

            static async Task<T2?> CaptureThrownException(Task<T2> task, List<Exception> exceptions)
            {
                try
                {
                    return await task;
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }

                return default;
            }
        }
    }
}
