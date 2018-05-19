using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
//using System.Runtime.Caching;
using System.Timers;
using DesertOctopus.MammothCache.Common;
//using Microsoft.Extensions.Caching.Memory;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Represents a cache that 'forget' items after a while
    /// </summary>
    public class SquirrelCache : IFirstLevelCache, IDisposable
    {
        private readonly IFirstLevelCacheConfig _config;
        private readonly IFirstLevelCacheCloningProvider _cloningProvider;
        private readonly IMammothCacheSerializationProvider _serializationProvider;

        private readonly InMemoryCache _cache;

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

            _cache = new InMemoryCache(_config.TimerInterval, _config.MaximumMemorySize);
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

            _isDisposed = true;
        }

        /// <summary>
        /// Gets the number of objects stored in the cache
        /// </summary>
        public int NumberOfObjects => _cache.Count;

        #region IFirstLevelCache

        public ConditionalResult<T> Get<T>(string key)
            where T : class
        {
            var cachedObject = _cache.Get(key);

            if (cachedObject?.Value != null)
            {
                if (_cloningProvider.RequireCloning(cachedObject.Value.GetType()))
                {
                    return ConditionalResult.CreateSuccessful(_cloningProvider.Clone(cachedObject.Value as T));
                }

                return ConditionalResult.CreateSuccessful(cachedObject.Value as T);

            }

            return ConditionalResult.CreateFailure<T>();
        }

        public void Set(string key,
                        byte[] serializedValue)
        {
            Set(key,
                serializedValue,
                null);
        }

        public void Set(string key,
                        byte[] serializedValue,
                        TimeSpan? ttl)
        {
            var policyTtl = _config.AbsoluteExpiration;
            if (ttl.HasValue
                && ttl.Value < _config.AbsoluteExpiration)
            {
                policyTtl = ttl.Value;
            }

            var co = new CachedObject
                     {
                         Key = key,
                         Value = _serializationProvider.Deserialize(serializedValue),
                         ObjectSize = serializedValue.Length,
                         ExpireAt = DateTime.UtcNow.Add(policyTtl)
                     };

            _cache.Add(key, co);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveAll()
        {
            _cache.RemoveAll();
        }

        #endregion

        /// <summary>
        /// Gets the estimated memory consumed by this instance of <see cref="SquirrelCache"/>
        /// </summary>
        public long EstimatedMemorySize => _cache.EstimatedMemorySize;
    }
}
