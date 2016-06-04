using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Represents the contract for classes that wishes to implement a serialization provider
    /// </summary>
    public interface IMammothCacheSerializationProvider
    {
        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="value">Object to serialize</param>
        /// <returns>Byte array representing the serialized object</returns>
        byte[] Serialize<T>(T value)
            where T : class;

        /// <summary>
        /// Deserialize the byte array to recreate the object.
        /// This method can only be used with deserializers that stores the type to deserialize in their payload.
        /// </summary>
        /// <param name="bytes">Byte array containing the serialized object</param>
        /// <returns>The deserialized object</returns>
        object Deserialize(byte[] bytes);

        /// <summary>
        /// Deserialize the byte array to recreate the object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="bytes">Byte array containing the serialized object</param>
        /// <returns>The deserialized object</returns>
        T Deserialize<T>(byte[] bytes)
            where T : class;

        /// <summary>
        /// Detect if a type can be serialized
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>True if the type can be serialized otherwise false</returns>
        bool CanSerialize(Type type);
    }
}