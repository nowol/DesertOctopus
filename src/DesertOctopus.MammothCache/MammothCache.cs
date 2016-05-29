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

        T GetOrAdd<T>(string key, Func<T> getAction, TimeSpan? ttl = null) where T : class;
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> getActionAsync, TimeSpan? ttl = null) where T : class;

        void Set<T>(string key, T value, TimeSpan? ttl = null) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class;

        void Remove(string key);
        Task RemoveAsync(string key);

        void RemoveAll();
        Task RemoveAllAsync();
    }

    public sealed class MammothCache : IMammothCache, IDisposable
    {
        private readonly IFirstLevelCache _firstLevelCache;
        private readonly ISecondLevelCache _secondLevelCache;
        private readonly IMammothCacheSerializationProvider _serializationProvider;
        private bool _isDisposed = false;

        public MammothCache(IFirstLevelCache firstLevelCache,
                            ISecondLevelCache secondLevelCache,
                            IMammothCacheSerializationProvider serializationProvider)
        {
            _firstLevelCache = firstLevelCache;
            _secondLevelCache = secondLevelCache;
            _serializationProvider = serializationProvider;

            SubscribeToEvents();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("This " + nameof(MammothCache) + " object is disposed.");
            }

            _secondLevelCache.OnItemRemovedFromCache -= OnItemRemovedFromSecondLevelCache;
        }

        private void SubscribeToEvents()
        {
            _secondLevelCache.OnItemRemovedFromCache += OnItemRemovedFromSecondLevelCache;
        }

        private void OnItemRemovedFromSecondLevelCache(string key)
        {
            _firstLevelCache.Remove(key);
        }

        public T Get<T>(string key) where T : class
        {
            var firstLevelResult = _firstLevelCache.Get<T>(key);
            if (firstLevelResult.IsSuccessful)
            {
                return firstLevelResult.Value;
            }

            var bytes = _secondLevelCache.Get(key);
            if (bytes != null)
            {
                var ttl = _secondLevelCache.GetTimeToLive(key);
                _firstLevelCache.Set(key, bytes, ttl: ttl);
                return _serializationProvider.Deserialize<T>(bytes);
            }
            return default(T);
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            var firstLevelResult = _firstLevelCache.Get<T>(key);
            if (firstLevelResult.IsSuccessful)
            {
                return firstLevelResult.Value;
            }
            var bytes = await _secondLevelCache.GetAsync(key).ConfigureAwait(false);
            if (bytes != null)
            {
                var ttl = await _secondLevelCache.GetTimeToLiveAsync(key).ConfigureAwait(false);
                _firstLevelCache.Set(key, bytes, ttl: ttl);
                return _serializationProvider.Deserialize<T>(bytes);
            }
            return default(T);
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

        public T GetOrAdd<T>(string key, Func<T> getAction, TimeSpan? ttl = null) 
            where T : class
        {
            if (getAction == null)
            {
                throw new ArgumentNullException(nameof(getAction));
            }

            var value = Get<T>(key);
            if (value != default(T))
            {
                return value;
            }

            value = getAction();
            if (value != default(T))
            {
                Set(key, value, ttl: ttl);
            }
            return value;
        }


        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> getActionAsync, TimeSpan? ttl = null) 
            where T : class
        {
            if (getActionAsync == null)
            {
                throw new ArgumentNullException(nameof(getActionAsync));
            }

            var value = await GetAsync<T>(key).ConfigureAwait(false);
            if (value != default(T))
            {
                return value;
            }

            value = await getActionAsync().ConfigureAwait(false);
            if (value != default(T))
            {
                await SetAsync(key, value, ttl: ttl).ConfigureAwait(false);
            }
            return value;
        }
    }
}
