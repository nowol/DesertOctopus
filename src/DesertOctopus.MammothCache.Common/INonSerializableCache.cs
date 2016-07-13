using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Represents the contract to implement for classes that wishes to provide a first level of cache
    /// </summary>
    public interface INonSerializableCache
    {
        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="key">Key of the object</param>
        /// <returns>The object wrapped in a <see cref="ConditionalResult{T}"/></returns>
        ConditionalResult<T> Get<T>(string key)
            where T : class;

        /// <summary>
        /// Remove an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        void Remove(string key);

        /// <summary>
        /// Remove all objects from the cache
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <param name="value">Non serializable object to store</param>
        void Set(string key, object value);

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <param name="value">Non serializable object to store</param>
        /// <param name="ttl">Optional time to live of the object</param>
        void Set(string key, object value, TimeSpan? ttl);

        /// <summary>
        /// Gets the number of items in the cache
        /// </summary>
        int NumberOfObjects { get; }
    }
}