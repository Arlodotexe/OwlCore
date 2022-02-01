namespace OwlCore.ComponentModel
{
    /// <summary>
    /// Indicates that the class is holding an a reference
    /// to an implementation of <typeparamref name="T"/>
    /// which properties, events or methods can be delegating to.
    /// </summary>
    /// <typeparam name="T">The type that is delegated to.</typeparam>
    public interface IDelegatable<T>
        where T : class
    {
        /// <summary>
        /// A wrapped implementation which member access can be delegated to.
        /// </summary>
        T Inner { get; }
    }

}
