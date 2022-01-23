namespace OwlCore.Remoting.Transfer
{
    /// <summary>
    /// Indicates the type of interception for a method call.
    /// </summary>
    public enum MethodCallInterceptionType
    {
        /// <summary>
        /// Indicates a method that has begun invocation.
        /// </summary>
        Entry,

        /// <summary>
        /// Indicates a method about to return.
        /// </summary>
        Exit,
    }
}
