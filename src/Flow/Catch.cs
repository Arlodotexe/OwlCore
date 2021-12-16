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
        /// Syntactic sugar for catching any exception in a callback with a return value.
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
        /// Syntactic sugar for catching a specific exception in a callback.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static void Catch<TException, TException2>(Action action)
            where TException : Exception
            where TException2 : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
            }
            catch (TException2)
            {
            }
        }

        /// <summary>
        /// Syntactic sugar for catching a specific exception in a callback.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static void Catch<TException, TException2, TException3>(Action action)
            where TException : Exception
            where TException2 : Exception
            where TException3 : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
            }
            catch (TException2)
            {
            }
            catch (TException3)
            {
            }
        }

        /// <summary>
        /// Syntactic sugar for catching a specific exception in a callback.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static void Catch<TException, TException2, TException3, TException4>(Action action)
            where TException : Exception
            where TException2 : Exception
            where TException3 : Exception
            where TException4 : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
            }
            catch (TException2)
            {
            }
            catch (TException3)
            {
            }
            catch (TException4)
            {
            }
        }

        /// <summary>
        /// Syntactic sugar for catching a specific exception in a callback with a return value.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Default (nullable) if the task fails.</returns>
        public static TResult? Catch<TResult, TException>(Func<TResult> func)
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
        /// Syntactic sugar for catching a specific exception in a callback with a return value.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Default (nullable) if the task fails.</returns>
        public static TResult? Catch<TResult, TException, TException2>(Func<TResult> func)
            where TException : Exception
            where TException2 : Exception
        {
            try
            {
                return func();
            }
            catch (TException)
            {
                return default;
            }
            catch (TException2)
            {
                return default;
            }
        }

        /// <summary>
        /// Syntactic sugar for catching a specific exception in a callback with a return value.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Default (nullable) if the task fails.</returns>
        public static TResult? Catch<TResult, TException, TException2, TException3>(Func<TResult> func)
            where TException : Exception
            where TException2 : Exception
            where TException3 : Exception
        {
            try
            {
                return func();
            }
            catch (TException)
            {
                return default;
            }
            catch (TException2)
            {
                return default;
            }
            catch (TException3)
            {
                return default;
            }
        }

        /// <summary>
        /// Syntactic sugar for catching a specific exception in a callback with a return value.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Default (nullable) if the task fails.</returns>
        public static TResult? Catch<TResult, TException, TException2, TException3, TException4>(Func<TResult> func)
            where TException : Exception
            where TException2 : Exception
            where TException3 : Exception
            where TException4 : Exception
        {
            try
            {
                return func();
            }
            catch (TException)
            {
                return default;
            }
            catch (TException2)
            {
                return default;
            }
            catch (TException3)
            {
                return default;
            }
            catch (TException4)
            {
                return default;
            }
        }
    }
}