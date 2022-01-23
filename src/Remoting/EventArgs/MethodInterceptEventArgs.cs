using System;
using System.Reflection;

namespace OwlCore.Remoting
{
    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="RemoteMethodAttribute.Entered"/>.
    /// </summary>
    public class MethodInterceptEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="MethodInterceptEventArgs"/>.
        /// </summary>
        /// <param name="methodCallId">A unique identifier for this method call.</param>
        /// <param name="declaringType">The type declaring the intercepted method.</param>
        /// <param name="instance">The instance of the class where the member is residing. Will be null if the member is static.</param>
        /// <param name="methodBase">Contains information about the method.</param>
        /// <param name="values">The passed arguments of the method.</param>
        public MethodInterceptEventArgs(string methodCallId, Type declaringType, object? instance, MethodBase methodBase, object?[] values)
        {
            MethodCallId = methodCallId;
            DeclaringType = declaringType;
            Instance = instance;
            MethodBase = methodBase;
            Values = values;
        }

        /// <summary>
        /// A unique identifier for this method call.
        /// </summary>
        public string MethodCallId { get; set; }

        /// <summary>
        /// The type declaring the intercepted method.
        /// </summary>
        public Type DeclaringType { get; }

        /// <summary>
        /// The instance of the class where the member is residing. Will be null if the member is static.
        /// </summary>
        public object? Instance { get; }

        /// <summary>
        /// Contains information about the method.
        /// </summary>
        public MethodBase MethodBase { get; }

        /// <summary>
        /// The passed arguments of the method.
        /// </summary>
        public object?[] Values { get; }
    }
}
