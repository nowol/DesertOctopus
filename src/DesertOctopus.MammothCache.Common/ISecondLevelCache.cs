using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Represents the contract for classes that wishes to implement a second level of caching
    /// </summary>
    public interface ISecondLevelCache
    {
        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>Byte array containing the serialized object</returns>
        byte[] Get(string key);

        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>Byte array containing the serialized object as an awaitable task</returns>
        Task<byte[]> GetAsync(string key);

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <param name="serializedValue">Byte array containing the serialized object</param>
        /// <param name="ttl">Optional time to live of the object</param>
        void Set(string key, byte[] serializedValue, TimeSpan? ttl = null);

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <param name="serializedValue">Byte array containing the serialized object</param>
        /// <param name="ttl">Optional time to live of the object</param>
        /// <returns>A task that can be awaited</returns>
        Task SetAsync(string key, byte[] serializedValue, TimeSpan? ttl = null);

        /// <summary>
        /// Remove an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>True if the object was removed. Otherwise false.</returns>
        bool Remove(string key);

        /// <summary>
        /// Remove an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>A task that can be awaited. True if the object was removed. Otherwise false.</returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// Remove all objects from the cache
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Remove all objects from the cache
        /// </summary>
        /// <returns>A task that can be awaited</returns>
        Task RemoveAllAsync();

        /// <summary>
        /// Get the time to live of an object
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>Time to live of the object</returns>
        TimeSpan? GetTimeToLive(string key);

        /// <summary>
        /// Get the time to live of an object
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>Time to live of the object as an awaitable task</returns>
        Task<TimeSpan?> GetTimeToLiveAsync(string key);

        /// <summary>
        /// Event launched when an object is removed from the cache
        /// </summary>
        event ItemEvictedFromCacheEventHandler OnItemRemovedFromCache;
    }
}