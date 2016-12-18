﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Timers;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Represents a cache that 'forget' items after a while
    /// </summary>
    public sealed class SquirrelCache : IFirstLevelCache, IDisposable
    {
        private readonly IFirstLevelCacheConfig _config;
        private readonly IFirstLevelCacheCloningProvider _cloningProvider;
        private readonly IMammothCacheSerializationProvider _serializationProvider;
        private readonly MemoryCache _cache = new MemoryCache("SquirrelCache");
        private readonly System.Threading.Timer _cleanUpTimer;
        private readonly CachedObjectQueue _cachedObjectsByAge = new CachedObjectQueue();
        private readonly LongCounter _estimatedMemorySize = new LongCounter();
        private bool _isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquirrelCache"/> class.
        /// </summary>
        /// <param name="config">Configuration of the <see cref="SquirrelCache"/></param>
        /// <param name="cloningProvider">Cloning provider</param>
        /// <param name="serializationProvider">Serialization provider</param>
        public SquirrelCache(IFirstLevelCacheConfig config, IFirstLevelCacheCloningProvider cloningProvider, IMammothCacheSerializationProvider serializationProvider)
        {
            _config = config;
            _cloningProvider = cloningProvider;
            _serializationProvider = serializationProvider;

            if (_config.AbsoluteExpiration.TotalSeconds <= 0)
            {
                throw new ArgumentException("AbsoluteExpiration must be greater than 0");
            }

            if (_config.MaximumMemorySize <= 0)
            {
                throw new ArgumentException("MaximumMemorySize must be greater than 0");
            }

            if (_config.TimerInterval.TotalMilliseconds <= 0)
            {
                throw new ArgumentException("TimerInterval must be greater than 0");
            }

            _cleanUpTimer = new System.Threading.Timer(CleanUpTimerOnElapsed, null, Convert.ToInt32(_config.TimerInterval.TotalMilliseconds), Timeout.Infinite);
        }

        private void CleanUpTimerOnElapsed(object state)
        {
            _cleanUpTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (EstimatedMemorySize > _config.MaximumMemorySize
                && _cachedObjectsByAge.Count > 0)
            {
                foreach (var key in _cachedObjectsByAge.GetKeysToEvictDueToMemory(EstimatedMemorySize - _config.MaximumMemorySize))
                {
                    Remove(key);
                }
            }

            _cleanUpTimer.Change(Convert.ToInt32(_config.TimerInterval.TotalMilliseconds), Timeout.Infinite);
        }

        /// <summary>
        /// Gets the number of objects stored in the cache
        /// </summary>
        public int NumberOfObjects
        {
            get
            {
                GuardDisposed();
                return (int)_cache.GetCount();
            }
        }

        /// <summary>
        /// Gets the estimated memory consumed by this instance of <see cref="SquirrelCache"/>
        /// </summary>
        public long EstimatedMemorySize
        {
            get { return _estimatedMemorySize.Get(); }
        }

        /// <inheritdoc/>
        public ConditionalResult<T> Get<T>(string key)
             where T : class
        {
            GuardDisposed();

            var value = _cache.Get(key) as CachedObject;
            if (value == null
                || value.Value == null)
            {
                return ConditionalResult.CreateFailure<T>();
            }

            if (_cloningProvider.RequireCloning(value.Value.GetType()))
            {
                return ConditionalResult.CreateSuccessful(_cloningProvider.Clone(value.Value as T));
            }

            return ConditionalResult.CreateSuccessful(value.Value as T);
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            GuardDisposed();
            _cache.Remove(key); // will trigger the RemovedCallback
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            GuardDisposed();
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                Remove(cacheKey);
            }
        }

        /// <inheritdoc/>
        public void Set(string key, byte[] serializedValue)
        {
            Set(key, serializedValue, null);
        }

        /// <inheritdoc/>
        public void Set(string key, byte[] serializedValue, TimeSpan? ttl)
        {
            GuardDisposed();
            Remove(key); // removing the item first to decrease the estimated memory usage

            var co = CreateCachedObject(key, serializedValue);

            var cacheItem = new CacheItem(key, co);
            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;
            if (ttl.HasValue && ttl.Value < _config.AbsoluteExpiration)
            {
                policy.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(ttl.Value);
            }
            else
            {
                policy.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(_config.AbsoluteExpiration);
            }

            policy.RemovedCallback += RemovedCallback;

            _cache.Set(cacheItem, policy);
            _cachedObjectsByAge.Add(co);

            _estimatedMemorySize.Add(co.ObjectSize);
        }

        private void RemovedCallback(CacheEntryRemovedArguments arguments)
        {
            var value = arguments.CacheItem.Value as CachedObject;
            if (value != null)
            {
                _estimatedMemorySize.Substract(value.ObjectSize);
                _cachedObjectsByAge.Remove(value);
            }
        }

        private CachedObject CreateCachedObject(string key, byte[] serializedValue)
        {
            var co = new CachedObject();
            co.Key = key;
            co.Value = _serializationProvider.Deserialize(serializedValue);
            co.ObjectSize = serializedValue.Length;

            return co;
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _cleanUpTimer.Dispose();
            _cache.Dispose();
            _cachedObjectsByAge.Dispose();
            _isDisposed = true;
        }

        private void GuardDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SquirrelCache));
            }
        }

        /// <summary>
        /// Gets the list of cached objects ordered by age.
        /// </summary>
        public CachedObjectQueue CachedObjectsByAge
        {
            get
            {
                GuardDisposed();
                return _cachedObjectsByAge;
            }
        }
    }
}
