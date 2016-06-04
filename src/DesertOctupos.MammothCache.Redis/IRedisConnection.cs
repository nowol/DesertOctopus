using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using StackExchange.Redis;

namespace DesertOctupos.MammothCache.Redis
{
    public interface IRedisConnection
    {
        byte[] Get(string key);
        Task<byte[]> GetAsync(string key);

        void Set(string key, byte[] serializedValue, TimeSpan? ttl = null);
        Task SetAsync(string key, byte[] serializedValue, TimeSpan? ttl = null);

        void Set(Dictionary<CacheItemDefinition, byte[]> objects);
        Task SetAsync(Dictionary<CacheItemDefinition, byte[]> objects);

        bool Remove(string key);
        Task<bool> RemoveAsync(string key);

        void RemoveAll();
        Task RemoveAllAsync();

        TimeSpan? GetTimeToLive(string key);
        Task<TimeSpan?> GetTimeToLiveAsync(string key);

        event ItemEvictedFromCacheEventHandler OnItemRemovedFromCache;

        KeyValuePair<string, string>[] GetConfig(string pattern);
        Task<KeyValuePair<string, string>[]> GetConfigAsync(string pattern);

        Dictionary<CacheItemDefinition, byte[]> Get(ICollection<CacheItemDefinition> keys);
        Task<Dictionary<CacheItemDefinition, byte[]>> GetAsync(ICollection<CacheItemDefinition> keys);

        
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