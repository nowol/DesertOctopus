using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache.Redis
{
    /// <summary>
    /// Represents the contract for classes that wishes to provide a connection to Redis
    /// </summary>
    public interface IRedisConnection
    {
        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>Byte array containing the serialized object</returns>
        byte[] Get(string key);

        /// <summary>
        /// Gets objects from the cache
        /// </summary>
        /// <param name="keys">Keys of the objects</param>
        /// <returns>Dictionary of byte array containing the serialized objects</returns>
        Dictionary<CacheItemDefinition, byte[]> Get(ICollection<CacheItemDefinition> keys);

        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>Byte array containing the serialized object as an awaitable task</returns>
        Task<byte[]> GetAsync(string key);

        /// <summary>
        /// Gets objects from the cache
        /// </summary>
        /// <param name="keys">Keys of the objects</param>
        /// <returns>Dictionary of byte array containing the serialized objects as an awaitable task</returns>
        Task<Dictionary<CacheItemDefinition, byte[]>> GetAsync(ICollection<CacheItemDefinition> keys);

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <param name="serializedValue">Byte array containing the serialized object</param>
        /// <param name="ttl">Optional time to live of the object</param>
        void Set(string key, byte[] serializedValue, TimeSpan? ttl = null);

        /// <summary>
        /// Store multiple objects in the cache
        /// </summary>
        /// <param name="objects">Objects to store in the cache</param>
        void Set(Dictionary<CacheItemDefinition, byte[]> objects);

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <param name="serializedValue">Byte array containing the serialized object</param>
        /// <param name="ttl">Optional time to live of the object</param>
        /// <returns>A task that can be awaited</returns>
        Task SetAsync(string key, byte[] serializedValue, TimeSpan? ttl = null);

        /// <summary>
        /// Store multiple objects in the cache
        /// </summary>
        /// <param name="objects">Objects to store in the cache</param>
        /// <returns>A task that can be awaited</returns>
        Task SetAsync(Dictionary<CacheItemDefinition, byte[]> objects);

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
        TimeToLiveResult GetTimeToLive(string key);

        /// <summary>
        /// Get the time to live of an object
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>Time to live of the object as an awaitable task</returns>
        Task<TimeToLiveResult> GetTimeToLiveAsync(string key);

        /// <summary>
        /// Get the time to live of many objects
        /// </summary>
        /// <param name="keys">Keys of the objects</param>
        /// <returns>Time to live of the objects</returns>
        Dictionary<string, TimeToLiveResult> GetTimeToLives(string[] keys);

        /// <summary>
        /// Get the time to live of many objects
        /// </summary>
        /// <param name="keys">Keys of the objects</param>
        /// <returns>Time to live of the objects</returns>
        Task<Dictionary<string, TimeToLiveResult>> GetTimeToLivesAsync(string[] keys);

        /// <summary>
        /// Event launched when an object is removed from the cache
        /// </summary>
        event ItemEvictedFromCacheEventHandler OnItemRemovedFromCache;

        /// <summary>
        /// Triggered when RemoveAll is called
        /// </summary>
        event RemoveAllItemsEventHandler OnRemoveAllItems;

        /// <summary>
        /// Get Redis' configuration
        /// </summary>
        /// <param name="pattern">Filter pattern</param>
        /// <returns>Redis' configuration</returns>
        KeyValuePair<string, string>[] GetConfig(string pattern);

        /// <summary>
        /// Get Redis' configuration
        /// </summary>
        /// <param name="pattern">Filter pattern</param>
        /// <returns>Redis' configuration as an awaitable task</returns>
        Task<KeyValuePair<string, string>[]> GetConfigAsync(string pattern);

        /// <summary>
        /// Check if a key exists in redis
        /// </summary>
        /// <param name="key">Key to look up</param>
        /// <returns>true if the key exists otherwise false</returns>
        bool KeyExists(string key);

        /// <summary>
        /// Check if a key exists in redis
        /// </summary>
        /// <param name="key">Key to look up</param>
        /// <returns>true if the key exists otherwise false as an awaitable task</returns>
        Task<bool> KeyExistsAsync(string key);

        /// <summary>
        /// Lock an object
        /// </summary>
        /// <param name="key">Key of the object to lock</param>
        /// <param name="lockExpiry">Time that the lock will be acquired</param>
        /// <param name="timeout">Timeout represents the time to wait for acquiring the lock</param>
        /// <returns>An object that must be disposed of to release the lock</returns>
        IDisposable AcquireLock(string key, TimeSpan lockExpiry, TimeSpan timeout);

        /// <summary>
        /// Lock an object
        /// </summary>
        /// <param name="key">Key of the object to lock</param>
        /// <param name="lockExpiry">Time that the lock will be acquired</param>
        /// <param name="timeout">Timeout represents the time to wait for acquiring the lock</param>
        /// <returns>An object that must be disposed of to release the lock as an awaitable task</returns>
        Task<IDisposable> AcquireLockAsync(string key, TimeSpan lockExpiry, TimeSpan timeout);
    }
}