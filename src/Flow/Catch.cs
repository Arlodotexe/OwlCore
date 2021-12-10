using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OwlCore
{
    /// <summary>
    /// Helper methods related to code flow.
    /// </summary>
    public static partial class Flow
    {
        /// <summary>
        /// Syntactic sugar for catching any exception in a callback.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static void Catch(Action action)
        {
            try
            {
                action();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Syntactic sugar for catching a specific exception in a callback.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static void Catch<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
            }
        }

        /// <summary>
        /// Syntactic sugar for catching a specific exception in a callback with a return value.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Default (nullable) if the task fails.</returns>
        public static TResult? Catch<TException, TResult>(Func<TResult> func)
            where TException : Exception
        {
            try
            {
                return func();
            }
            catch (TException)
            {
                return default;
            }
        }

        /// <summary>
        /// Syntactic sugar for catching an exception in a callback with a return value.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Default (nullable) if the task fails.</returns>
        public static TResult? Catch<TResult>(Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return default;
            }
        }
    }
}