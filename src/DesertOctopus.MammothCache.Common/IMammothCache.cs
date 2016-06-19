using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Represents the contract to implements for classes that wishes to provide caching
    /// </summary>
    public interface IMammothCache
    {
        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="key">Key of the object</param>
        /// <returns>The object or default({T}) if the object was not found</returns>
        T Get<T>(string key)
            where T : class;

        /// <summary>
        /// Gets multiple objects from the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="keys">Keys of the objects</param>
        /// <returns>A dictionary containing the objects that exists in the cache</returns>
        Dictionary<CacheItemDefinition, T> Get<T>(ICollection<CacheItemDefinition> keys)
            where T : class;

        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="key">Key of the object</param>
        /// <returns>The object or default({T}) if the object was not found as an awaitable task</returns>
        Task<T> GetAsync<T>(string key)
            where T : class;

        /// <summary>
        /// Gets multiple objects from the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="keys">Keys of the objects</param>
        /// <returns>A dictionary containing the objects that exists in the cache as an awaitable task</returns>
        Task<Dictionary<CacheItemDefinition, T>> GetAsync<T>(ICollection<CacheItemDefinition> keys)
            where T : class;

        /// <summary>
        /// Gets an object from the cache.  If the object does not exists, getAction is executed and its result, if not null, is stored in the cache.
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="key">Key of the object</param>
        /// <param name="getAction">Delegate that will create the object</param>
        /// <param name="ttl">Optional time to live of the object</param>
        /// <returns>The object or default({T}) if the object was not found</returns>
        T GetOrAdd<T>(string key, Func<T> getAction, TimeSpan? ttl = null)
            where T : class;

        /// <summary>
        /// Gets multiple objects from the cache.  If the objects does not exist, getAction is executed and its results, if not null, is stored in the cache.
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="keys">Keys of the objects</param>
        /// <param name="getAction">Delegate that will create the objects</param>
        /// <returns>The objects retrieved from the cache</returns>
        Dictionary<CacheItemDefinition, T> GetOrAdd<T>(IEnumerable<CacheItemDefinition> keys, Func<CacheItemDefinition[], Dictionary<CacheItemDefinition, T>> getAction)
            where T : class;

        /// <summary>
        /// Gets an object from the cache.  If the object does not exists, getAction is executed and its result, if not null, is stored in the cache.
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="key">Key of the object</param>
        /// <param name="getActionAsync">Delegate that will create the object</param>
        /// <param name="ttl">Optional time to live of the object</param>
        /// <returns>The object or default({T}) if the object was not found as an awaitable task</returns>
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> getActionAsync, TimeSpan? ttl = null)
            where T : class;

        /// <summary>
        /// Gets multiple objects from the cache.  If the objects does not exist, getAction is executed and its results, if not null, is stored in the cache.
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="keys">Keys of the objects</param>
        /// <param name="getActionAsync">Delegate that will create the objects</param>
        /// <returns>The objects retrieved from the cache as an awaitable task</returns>
        Task<Dictionary<CacheItemDefinition, T>> GetOrAddAsync<T>(IEnumerable<CacheItemDefinition> keys, Func<CacheItemDefinition[], Task<Dictionary<CacheItemDefinition, T>>> getActionAsync)
            where T : class;

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="key">Key of the object</param>
        /// <param name="value">Object to store</param>
        /// <param name="ttl">Optional time to live of the object</param>
        void Set<T>(string key, T value, TimeSpan? ttl = null)
            where T : class;

        /// <summary>
        /// Store multiple objects in the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="objects">Object to store</param>
        void Set<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class;

        /// <summary>
        /// Store an object in the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="key">Key of the object</param>
        /// <param name="value">Object to store</param>
        /// <param name="ttl">Optional time to live of the object</param>
        /// <returns>A task that can be awaited</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
            where T : class;

        /// <summary>
        /// Store multiple objects in the cache
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="objects">Object to store</param>
        /// <returns>A task that can be awaited</returns>
        Task SetAsync<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class;

        /// <summary>
        /// Remove an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        void Remove(string key);

        /// <summary>
        /// Remove an object from the cache
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>A task that can be awaited</returns>
        Task RemoveAsync(string key);

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