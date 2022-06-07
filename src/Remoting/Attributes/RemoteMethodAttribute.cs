using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cauldron.Interception;
using CommunityToolkit.Diagnostics;

namespace OwlCore.Remoting
{
    /// <summary>
    /// Mark a method or class with this attribute to opt into remote method invocation via <see cref="MemberRemote"/>.
    /// </summary>
    /// <remarks>
    /// For IL weaving to take effect, you must install and add a reference to <see href="https://www.nuget.org/packages/Cauldron.BasicInterceptors/3.2.3">Cauldron.BasicInterceptors</see> directly in the project that uses this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    [InterceptorOptions(AlwaysCreateNewInstance = true)]
    public class RemoteMethodAttribute : Attribute, IMethodInterceptor
    {
        private MethodInterceptEventArgs? _methodEnteredData;

        // Semaphore used to ensure the Entered event finishes execution before the Exited event is raised.
        private SemaphoreSlim _interceptSemaphore = new SemaphoreSlim(1, 1);
        private bool _isInvokedInternally;
        private bool _isEventAddOrRemove;

        /// <inheritdoc/>
        public void OnEnter(Type declaringType, object instance, MethodBase methodbase, object[] values)
        {
            _interceptSemaphore.Wait();

            if (MethodIsEventHandlerAdd(methodbase) || MethodIsEventHandlerRemove(methodbase))
            {
                _isEventAddOrRemove = true;
                _interceptSemaphore.Release();
                return;
            }

            // Check if the invoker was the OwlCore.Remoting library.
            // Don't re-emit "entered" to the library if so.
            lock (MemberRemote.MemberHandleExpectancyMap)
            {
                if (MemberRemote.MemberHandleExpectancyMap.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var expectedInstance) && ReferenceEquals(expectedInstance, instance))
                {
                    MemberRemote.MemberHandleExpectancyMap.Remove(Thread.CurrentThread.ManagedThreadId);

                    _isInvokedInternally = true;
                    _interceptSemaphore.Release();
                    return;
                }
            }

            _methodEnteredData = new MethodInterceptEventArgs(Guid.NewGuid().ToString(), declaringType, instance, methodbase, values);
            Entered?.Invoke(this, _methodEnteredData);

            _interceptSemaphore.Release();
        }

        /// <inheritdoc/>
        public bool OnException(Exception e)
        {
            _interceptSemaphore.Wait();

            ExceptionRaised?.Invoke(this, e);

            _interceptSemaphore.Release();
            return true;
        }

        /// <inheritdoc/>
        public void OnExit()
        {
            if (_isInvokedInternally || _isEventAddOrRemove)
                return;

            _interceptSemaphore.Wait();

            // This data should exist if not invoked internally.
            Guard.IsNotNull(_methodEnteredData, nameof(_methodEnteredData));

            Exited?.Invoke(this, _methodEnteredData);

            _interceptSemaphore.Release();
        }

        /// <summary>
        /// Raised when the attached method is fired.
        /// </summary>
        /// <remarks>
        /// This static needed because the <see cref="IMethodInterceptor"/> weaver removes the attribute from the method in IL, making it innaccessible through normal means.
        /// Since this event emits the same instance we pass to <see cref="MemberRemote"/>, we can still use this.
        /// </remarks>
        public static event EventHandler<MethodInterceptEventArgs>? Entered;

        /// <summary>
        /// Raised when the event is finished executing and is about to exit.
        /// </summary>
        /// <remarks>
        /// This static needed because the <see cref="IMethodInterceptor"/> weaver removes the attribute from the method in IL, making it innaccessible through normal means.
        /// Since this event emits the same instance we pass to <see cref="MemberRemote"/>, we can still use this.
        /// </remarks>
        public static event EventHandler<MethodInterceptEventArgs>? Exited;

        /// <summary>
        /// Raised when an exception occurs in the method.
        /// </summary>
        /// <remarks>
        /// This static needed because the <see cref="IMethodInterceptor"/> weaver removes the attribute from the method in IL, making it innaccessible through normal means.
        /// Since this event emits the same instance we pass to <see cref="MemberRemote"/>, we can still use this.
        /// </remarks>
        public static event EventHandler<Exception>? ExceptionRaised;

        private bool MethodIsEventHandlerAdd(MethodBase methodbase)
        {
            var events = methodbase.DeclaringType.GetEvents();

            foreach (var ev in events)
                if (ev.GetAddMethod() == methodbase)
                    return true;

            return false;
        }

        private bool MethodIsEventHandlerRemove(MethodBase methodbase)
        {
            var events = methodbase.DeclaringType.GetEvents();

            foreach (var ev in events)
                if (ev.GetRemoveMethod() == methodbase)
                    return true;

            return false;
        }
    }
}
