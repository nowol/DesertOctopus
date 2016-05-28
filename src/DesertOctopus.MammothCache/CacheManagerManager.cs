using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache
{
    /*

         Distributed cache
             IDistributedCacheProvider
                 Redis impl
                     Retry mechanisim / https://github.com/App-vNext/Polly /  Enterprise lib

         max ram usage

         ttl 
             multiple generation
                 before removing an item check if it has changed and if it does not extend ttl (using remaining ttl or something)
             weakreferences?

         clone obj when retrieving (ICloneFromCacheProvider)
             optional cloning
             make 3 'modes':
                 always clone
                 never clone
                 selective clone

         key expiration subscriber

         support multiple serializers?

         logging

         perf counters?

         stats? number of times an item is get/set
             only keep stats for objects in memory?
             for all object and forever? -- could be considered a 'memory leak' but this information is small
                 optionally set a remove after X minutes

         Crazy idea: dynamically create proxy of all objects and make them immutable



     ------

     project structure
         MC.Core
         MC.Interfaces
         MC.Redis

        renommer MammothCache a Caching

      */


    //public interface IMammothCacheManager
    //{
    //    T Get<T>(string key) where T : class;

    //    void Set<T>(string key, T value, TimeSpan? ttl = null) where T : class;

    //    void Remove(string key);
    //    Task RemoveAsync(string key);

    //    void RemoveAll();
    //}

    public delegate void ItemEvictedFromCacheEventHandler(string key);

    public interface ICommonCache
    {
        T Get<T>(string key) where T : class;
        Task<T> GetAsync<T>(string key) where T : class;

        void Set<T>(string key, T value, TimeSpan? ttl = null) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class;

        void Remove(string key);
        Task RemoveAsync(string key);

        void RemoveAll();
        Task RemoveAllAsync();

    }

    public interface ICacheRepository : ICommonCache
    {

        bool GetTimeToLive(string key, out TimeSpan? ttl);
        Task<bool> GetTimeToLiveAsync(string key, out TimeSpan? ttl);
        event ItemEvictedFromCacheEventHandler OnItemEvictedFromCache;
    }

    public interface IMammothCacheManager : ICommonCache
    {

    }


    /// <summary>
    /// The MammothCacheManager manages multiple level of cache.
    /// We go through the differents levels until we find the item corresponding to a given key.
    /// </summary>
    public class MammothCacheManager : IMammothCacheManager
    {
        private readonly ICacheRepository[] _cacheLevels;
        private readonly int _numberOfCacheLevels;

        /// <summary>
        /// Initialize an instance of <see cref="MammothCacheManager"/>
        /// </summary>
        /// <param name="cacheLevels">Cache levels to use. The levels are queried in the order they are sent to the constructor.</param>
        public MammothCacheManager(IEnumerable<ICacheRepository> cacheLevels)
        {
            if (cacheLevels == null)
            {
                throw new ArgumentNullException(nameof(cacheLevels));
            }

            _cacheLevels = cacheLevels.ToArray();
            _numberOfCacheLevels = _cacheLevels.Length;

            if (_numberOfCacheLevels == 0)
            {
                throw new ArgumentException(nameof(cacheLevels) + " cannot be empty.");
            }

            SubscribeToCacheEvictionEvents();
        }

        private void SubscribeToCacheEvictionEvents()
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                var cacheLevelIndex = i;
                cache.OnItemEvictedFromCache += key => { CacheOnOnItemEvictedFromCache(key, cacheLevelIndex); };
            }
        }

        private void CacheOnOnItemEvictedFromCache(string key, int cacheLevelIndex)
        {
            for (int i = cacheLevelIndex - 1; i >= 0; i--)
            {
                var cache = _cacheLevels[i];
                cache.Remove(key);
            }
        }

        public T Get<T>(string key)
             where T : class
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                var cachedItem = cache.Get<T>(key);
                if (cachedItem != null)
                {
                    SaveObjectToLowerLevelCaches(key, cachedItem, i);
                    return cachedItem;
                }
            }

            return default(T);
        }

        public async Task<T> GetAsync<T>(string key)
             where T : class
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                var cachedItem = await cache.GetAsync<T>(key).ConfigureAwait(false);
                if (cachedItem != null)
                {
                    await SaveObjectToLowerLevelCachesAsync(key, cachedItem, i).ConfigureAwait(false);
                    return cachedItem;
                }
            }

            return default(T);
        }

        private void SaveObjectToLowerLevelCaches<T>(string key,
                                                     T cachedItem,
                                                     int cacheLevelWhereObjectWasFound)
             where T : class
        {
            var originalCache = _cacheLevels[cacheLevelWhereObjectWasFound];
            TimeSpan? ttl;
            if (originalCache.GetTimeToLive(key, out ttl))
            {
                for (int i = cacheLevelWhereObjectWasFound - 1; i >= 0; i--)
                {
                    var cache = _cacheLevels[i];
                    cache.Set(key, cachedItem, ttl);
                }
            }
        }

        private async Task SaveObjectToLowerLevelCachesAsync<T>(string key,
                                                                T cachedItem,
                                                                int cacheLevelWhereObjectWasFound)
             where T : class
        {
            var originalCache = _cacheLevels[cacheLevelWhereObjectWasFound];
            TimeSpan? ttl;
            if (await originalCache.GetTimeToLiveAsync(key, out ttl).ConfigureAwait(false))
            {
                for (int i = cacheLevelWhereObjectWasFound - 1; i >= 0; i--)
                {
                    var cache = _cacheLevels[i];
                    await cache.SetAsync(key, cachedItem, ttl).ConfigureAwait(false);
                }
            }
        }

        public void Remove(string key)
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                cache.Remove(key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                await cache.RemoveAsync(key).ConfigureAwait(false);
            }
        }

        public void RemoveAll()
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                cache.RemoveAll();
            }
        }

        public async Task RemoveAllAsync()
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                await cache.RemoveAllAsync().ConfigureAwait(false);
            }
        }

        public void Set<T>(string key,
                           T value,
                           TimeSpan? ttl = null) 
            where T : class
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                cache.Set(key, value, ttl: ttl);
            }
        }

        public async Task SetAsync<T>(string key,
                                      T value,
                                      TimeSpan? ttl = null) 
            where T : class
        {
            for (int i = 0; i < _numberOfCacheLevels; i++)
            {
                var cache = _cacheLevels[i];
                await cache.SetAsync(key, value, ttl: ttl).ConfigureAwait(false);
            }
        }
    }


    public sealed class MemoryCacheRepository : ICacheRepository, IDisposable
    {
        public event ItemEvictedFromCacheEventHandler OnItemEvictedFromCache;

        private readonly MemoryCache _cache;
        private volatile bool _isDisposed = false;

        public MemoryCacheRepository()
        {
            _cache = new MemoryCache("MemoryCacheRepository");
        }

        public void Dispose()
        {
            GuardDisposed();
            _cache.Dispose();
            _isDisposed = true;
        }

        private void GuardDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(MemoryCacheRepository));
            }
        }

        public T Get<T>(string key) where T : class
        {
            GuardDisposed();
            return _cache.Get(key) as T;
        }

        public Task<T> GetAsync<T>(string key) where T : class
        {
            return Task.FromResult(Get<T>(key));
        }

        public void Set<T>(string key,
                           T value,
                           TimeSpan? ttl = null) where T : class
        {
            GuardDisposed();
            
            var cacheItem = new CacheItem(key, value);
            var policy = new CacheItemPolicy();
            if (ttl.HasValue)
            {
                policy.Priority = CacheItemPriority.Default;
                policy.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(ttl.Value);
            }
            else
            {
                policy.Priority = CacheItemPriority.NotRemovable;
            }

            policy.RemovedCallback = arguments => { SendOnItemEvictedFromCache(arguments.CacheItem.Key); };


            _cache.Set(cacheItem, policy);
        }

        public Task SetAsync<T>(string key,
                                T value,
                                TimeSpan? ttl = null) where T : class
        {
            Set<T>(key, value, ttl: ttl);
            return Task.FromResult(true);
        }

        public void Remove(string key)
        {
            GuardDisposed();
            _cache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.FromResult(true);
        }

        public void RemoveAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                Remove(cacheKey);
            }
        }

        public Task RemoveAllAsync()
        {
            RemoveAll();
            return Task.FromResult(true);
        }

        private void SendOnItemEvictedFromCache(string key)
        {
            var handlerCopy = OnItemEvictedFromCache;
            if (handlerCopy != null)
            {
                handlerCopy(key);
            }
        }

        public bool GetTimeToLive(string key,
                                  out TimeSpan? ttl)
        {
            var cacheItem = _cache.GetCacheItem(key);
            if (cacheItem == null)
            {
                ttl = null;
                return false;
            }

            ttl = null;
            return true;
        }

        public Task<bool> GetTimeToLiveAsync(string key,
                                       out TimeSpan? ttl)
        {
            return Task.FromResult(GetTimeToLive(key, out ttl));
        }

    }


}
