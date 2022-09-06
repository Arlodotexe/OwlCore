using System;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.ComponentModel
{
    /// <summary>
    /// An interface that allows serializing data to and from <typeparamref name="TSerialized"/> asynchronously.
    /// </summary>
    /// <typeparam name="TSerialized">The type that data is serialized to.</typeparam>
    public interface IAsyncSerializer<TSerialized>
    {
        /// <summary>
        /// Serializes the provided <paramref cref="data"/> into <typeparam name="TSerialized"/>.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <typeparam name="T">The type of the object being serialized.</typeparam>
        /// <returns>A serialized instance of <paramref name="data"/>.</returns>
        public Task<TSerialized> SerializeAsync<T>(T data, CancellationToken? cancellationToken = null);
        
        /// <summary>
        /// Serializes the provided <paramref cref="data"/> into <typeparam name="TSerialized"/>.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <param name="inputType">The type of the <paramref name="data" /> object being serialized.</param>
        /// <returns>A serialized instance of <paramref name="data"/>.</returns>
        public Task<TSerialized> SerializeAsync(Type inputType, object data, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Deserializes the provided <paramref name="serialized"/> data into the given type.
        /// </summary>
        /// <param name="serialized">A serialized instance to deserialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <typeparam name="TResult">The type to deserialize to.</typeparam>
        /// <returns>A deserialized instance of the provided serialized data.</returns>
        public Task<TResult> DeserializeAsync<TResult>(TSerialized serialized, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Deserializes the provided <paramref name="serialized"/> data into the given type.
        /// </summary>
        /// <param name="serialized">A serialized instance to deserialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <param name="returnType">The return type of the object being deserialized.</param>
        /// <returns>A deserialized instance of the provided serialized data.</returns>
        public Task<object> DeserializeAsync(Type returnType, TSerialized serialized, CancellationToken? cancellationToken = null);
    }
    
    /// <summary>
    /// An interface that allows serializing data to <typeparamref name="TDeserialized"/> and from <typeparamref name="TSerialized"/> asynchronously.
    /// </summary>
    /// <typeparam name="TSerialized">The type that data is serialized to.</typeparam>
    public interface IAsyncSerializer<TSerialized, TDeserialized>
    {
        /// <summary>
        /// Serializes the provided <paramref cref="data"/> into <typeparam name="TSerialized"/>.
        /// </summary>
        /// <param name="data">The object instance to serialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <returns>A serialized instance of <paramref name="data"/>.</returns>
        public Task<TSerialized> SerializeAsync(TDeserialized data, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Deserializes the provided <paramref name="serialized"/> data into the given type.
        /// </summary>
        /// <param name="serialized">A serialized instance to deserialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <returns>A deserialized instance of the provided serialized data.</returns>
        public Task<TDeserialized> DeserializeAsync(TSerialized serialized, CancellationToken? cancellationToken = null);
    }
}