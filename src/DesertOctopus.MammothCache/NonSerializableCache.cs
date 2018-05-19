using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Represents a cache for objects that cannot be serialized
    /// </summary>
    public sealed class NonSerializableCache : INonSerializableCache, IDisposable
    {
        private readonly InMemoryCache _cache = new InMemoryCache(Timeout.InfiniteTimeSpan, Int32.MaxValue);

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public ConditionalResult<T> Get<T>(string key)
            where T : class
        {
            CachedObject cachedObject = _cache.Get(key);

            if (cachedObject == null)
            {
                return ConditionalResult.CreateFailure<T>();
            }

            if (cachedObject.Value == null)
            {
                return ConditionalResult.CreateSuccessful<T>(null);
            }

            var castedValue = cachedObject.Value as T;
            if (castedValue == null)
            {
                return ConditionalResult.CreateFailure<T>();
            }

            return ConditionalResult.CreateSuccessful(castedValue);
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            _cache.RemoveAll();
        }

        /// <inheritdoc/>
        public void Set(string key,
                        object value,
                        TimeSpan? ttl)
        {
            DateTime? policyTtl = null;
            if (ttl.HasValue)
            {
                policyTtl = DateTime.UtcNow.Add(ttl.Value);
            }

            var co = new CachedObject
                     {
                         Key = key,
                         Value = value,
                         ObjectSize = 0,
                         ExpireAt = policyTtl
                     };

            _cache.Add(key, co);
        }

        /// <inheritdoc/>
        public int NumberOfObjects => _cache.Count;
    }
}
