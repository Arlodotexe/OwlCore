namespace OwlCore.Services
{
    /// <summary>
    /// An interface that allows serializing data to and from <typeparamref name="TSerialized"/> synchronously.
    /// </summary>
    /// <typeparam name="TSerialized">The type that data is serialized to.</typeparam>
    public interface ISerializer<TSerialized>
    {
        /// <summary>
        /// Serializes the provided <paramref cref="data"/> into <typeparam name="TSerialized"/>.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <typeparam name="T">The type of the object being serialized.</typeparam>
        /// <returns>A serialized instance of <paramref name="data"/>.</returns>
        public TSerialized Serialize<T>(T data);

        /// <summary>
        /// Deserializes the provided <paramref name="serialized"/> data into the given type.
        /// </summary>
        /// <param name="serialized">A serialized instance to deserialize.</param>
        /// <typeparam name="TResult">The type to deserialize to.</typeparam>
        /// <returns>A deserialized instance of the provided serialized data.</returns>
        public TResult Deserialize<TResult>(TSerialized serialized);
    }
    
    /// <summary>
    /// An interface that allows serializing data to <typeparamref name="TDeserialized"/> and from <typeparamref name="TSerialized"/> synchronously.
    /// </summary>
    /// <typeparam name="TSerialized">The type that data is serialized to.</typeparam>
    public interface ISerializer<TSerialized, TDeserialized>
    {
        /// <summary>
        /// Serializes the provided <paramref cref="data"/> into <typeparam name="TSerialized"/>.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <returns>A serialized instance of <paramref name="data"/>.</returns>
        public TSerialized Serialize(TDeserialized data);

        /// <summary>
        /// Deserializes the provided <paramref name="serialized"/> data into the given type.
        /// </summary>
        /// <param name="serialized">A serialized instance to deserialize.</param>
        /// <returns>A deserialized instance of the provided serialized data.</returns>
        public TDeserialized Deserialize(TSerialized serialized);
    }
}