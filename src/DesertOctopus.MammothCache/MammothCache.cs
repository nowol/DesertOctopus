using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    public interface IMammothCache
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

    public interface IMammothCacheSerializationProvider
    {
        byte[] Serialize<T>(T value) where T : class;
        object Deserialize(byte[] bytes);
        T Deserialize<T>(byte[] bytes) where T : class;
    }

    public class MammothCache : IMammothCache
    {
        private readonly IFirstLevelCache _firstLevelCache;
        private readonly ISecondLevelCache _secondLevelCache;
        private readonly IMammothCacheSerializationProvider _serializationProvider;

        public MammothCache(IFirstLevelCache firstLevelCache, 
                            ISecondLevelCache secondLevelCache,
                            IMammothCacheSerializationProvider serializationProvider)
        {
            _firstLevelCache = firstLevelCache;
            _secondLevelCache = secondLevelCache;
            _serializationProvider = serializationProvider;
        }

        public T Get<T>(string key) where T : class
        {
            var firstLevelResult = _firstLevelCache.Get<T>(key);
            if (firstLevelResult.IsSuccessful)
            {
                return firstLevelResult.Value;
            }

            return _secondLevelCache.Get<T>(key);
        }

        public Task<T> GetAsync<T>(string key) where T : class
        {
            var firstLevelResult = _firstLevelCache.Get<T>(key);
            if (firstLevelResult.IsSuccessful)
            {
                return Task.FromResult(firstLevelResult.Value);
            }

            return _secondLevelCache.GetAsync<T>(key);
        }

        public void Set<T>(string key,
                           T value,
                           TimeSpan? ttl = null) where T : class
        {
            if (value == null)
            {
                return;
            }

            var bytes = _serializationProvider.Serialize(value);

            _firstLevelCache.Set(key, bytes, ttl: ttl);
            _secondLevelCache.Set(key, bytes, ttl: ttl);
        }

        public Task SetAsync<T>(string key,
                                T value,
                                TimeSpan? ttl = null) where T : class
        {
            if (value == null)
            {
                return Task.FromResult(true);
            }

            var bytes = _serializationProvider.Serialize(value);

            _firstLevelCache.Set(key, bytes, ttl: ttl);
            return _secondLevelCache.SetAsync(key, bytes, ttl: ttl);
        }

        public void Remove(string key)
        {
            _firstLevelCache.Remove(key);
            _secondLevelCache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            _firstLevelCache.Remove(key);
            return _secondLevelCache.RemoveAsync(key);
        }

        public void RemoveAll()
        {
            _firstLevelCache.RemoveAll();
            _secondLevelCache.RemoveAll();
        }

        public Task RemoveAllAsync()
        {
            _firstLevelCache.RemoveAll();
            return _secondLevelCache.RemoveAllAsync();
        }
    }
}
