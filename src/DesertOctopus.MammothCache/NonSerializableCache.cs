using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Represents a cache for objects that cannot be serialized
    /// </summary>
    public sealed class NonSerializableCache : INonSerializableCache, IDisposable
    {
        private readonly MemoryCache _cache = new MemoryCache("NonSerializableCache");

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            _cache.Dispose();
        }

        /// <inheritdoc/>
        public ConditionalResult<T> Get<T>(string key)
            where T : class
        {
            var value = _cache.Get(key) as T;
            if (value == null)
            {
                return ConditionalResult.CreateFailure<T>();
            }

            return ConditionalResult.CreateSuccessful(value);
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                Remove(cacheKey);
            }
        }

        /// <inheritdoc/>
        public void Set(string key,
                        object value,
                        TimeSpan? ttl = null)
        {
            if (value == null)
            {
                return;
            }

            Remove(key); // removing the item first to decrease the estimated memory usage

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

            _cache.Set(cacheItem, policy);
        }

        /// <inheritdoc/>
        public int NumberOfObjects
        {
            get { return (int)_cache.GetCount(); }
        }
    }
}
