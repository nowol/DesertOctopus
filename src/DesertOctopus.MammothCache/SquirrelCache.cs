using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DesertOctopus.MammothCache
{

    public interface IFirstLevelCache
    {
        T Get<T>(string key)
            where T : class;

        void Remove(string key);
        void RemoveAll();

        void Set(string key, byte[] serializedValue);
    }

    /// <summary>
    /// Represents a cache that 'forget' items after a while
    /// </summary>
    public sealed class SquirrelCache : IFirstLevelCache, IDisposable
    {
        private readonly FirstLevelCacheConfig _config;
        private readonly MemoryCache _cache = new MemoryCache("SquirrelCache");
        private readonly System.Timers.Timer _cleanUpTimer;
        private readonly CachedObjectQueue _cachedObjectsByAge = new CachedObjectQueue();

        public SquirrelCache(FirstLevelCacheConfig config)
        {
            _config = config;

            _cleanUpTimer = new Timer(_config.TimerInterval);
            _cleanUpTimer.Elapsed += CleanUpTimerOnElapsed;
            _cleanUpTimer.AutoReset = true;
            _cleanUpTimer.Start();
        }

        private void CleanUpTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _cleanUpTimer.Stop();

            while (EstimatedMemorySize >= _config.MaximumMemorySize
                    && _cachedObjectsByAge.Count > 0)
            {
                var firstItem = _cachedObjectsByAge.Pop();
                if (firstItem != null)
                {
                    Remove(firstItem.Key);
                }
            }

            _cleanUpTimer.Start();
        }

        public int NumberOfObjects { get { return (int)_cache.GetCount(); } }
        public int EstimatedMemorySize { get; private set; }

        public T Get<T>(string key)
             where T : class
        {
            var value = _cache.Get(key) as CachedObject;
            if (value == null
                || value.Value == null)
            {
                return default(T);
            }

            return value.Value as T;
        }

        public void Remove(string key)
        {
            _cache.Remove(key); // will trigger the RemovedCallback
        }

        public void RemoveAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                Remove(cacheKey);
            }
        }

        public void Set(string key, byte[] serializedValue)
        {
            Remove(key); // removing the item first to decrease the estimated memory usage

            var co = CreateCachedObject(key, serializedValue);

            var cacheItem = new CacheItem(key, co);
            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;
            policy.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(_config.AbsoluteExpiration);
            policy.RemovedCallback += RemovedCallback;

            _cache.Set(cacheItem, policy);
            _cachedObjectsByAge.Add(co);

            EstimatedMemorySize += co.ObjectSize;
        }

        private void RemovedCallback(CacheEntryRemovedArguments arguments)
        {
            var value = arguments.CacheItem.Value as CachedObject;
            if (value != null)
            {
                EstimatedMemorySize -= value.ObjectSize;
                _cachedObjectsByAge.Remove(value);
            }
        }

        private CachedObject CreateCachedObject(string key, byte[] serializedValue)
        {
            var co = new CachedObject();
            co.Key = key;
            co.Value = KrakenSerializer.Deserialize(serializedValue);
            co.ObjectSize = serializedValue.Length;

            return co;
        }

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SquirrelCache));
            }
            _cache.Dispose();
        }
    }
}
