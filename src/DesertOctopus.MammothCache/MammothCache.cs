using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Distributed cache with 2 level of caching
    /// </summary>
    public sealed class MammothCache : IMammothCache, IDisposable
    {
        private readonly IFirstLevelCache _firstLevelCache;
        private readonly ISecondLevelCache _secondLevelCache;
        private readonly IMammothCacheSerializationProvider _serializationProvider;
        private bool _isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MammothCache"/> class.
        /// </summary>
        /// <param name="firstLevelCache">First level of cache</param>
        /// <param name="secondLevelCache">Second level of cache</param>
        /// <param name="serializationProvider">Serialization provider</param>
        public MammothCache(IFirstLevelCache firstLevelCache,
                            ISecondLevelCache secondLevelCache,
                            IMammothCacheSerializationProvider serializationProvider)
        {
            _firstLevelCache = firstLevelCache;
            _secondLevelCache = secondLevelCache;
            _serializationProvider = serializationProvider;

            SubscribeToEvents();
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
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

        /// <inheritdoc/>
        public T Get<T>(string key)
            where T : class
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

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key)
            where T : class
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

        /// <inheritdoc/>
        public void Set<T>(string key,
                           T value,
                           TimeSpan? ttl = null)
            where T : class
        {
            if (value == null)
            {
                return;
            }

            var bytes = _serializationProvider.Serialize(value);

            _firstLevelCache.Set(key, bytes, ttl: ttl);
            _secondLevelCache.Set(key, bytes, ttl: ttl);
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key,
                                T value,
                                TimeSpan? ttl = null)
            where T : class
        {
            if (value == null)
            {
                return Task.FromResult(true);
            }

            var bytes = _serializationProvider.Serialize(value);

            _firstLevelCache.Set(key, bytes, ttl: ttl);
            return _secondLevelCache.SetAsync(key, bytes, ttl: ttl);
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            _firstLevelCache.Remove(key);
            _secondLevelCache.Remove(key);
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key)
        {
            _firstLevelCache.Remove(key);
            return _secondLevelCache.RemoveAsync(key);
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            _firstLevelCache.RemoveAll();
            _secondLevelCache.RemoveAll();
        }

        /// <inheritdoc/>
        public Task RemoveAllAsync()
        {
            _firstLevelCache.RemoveAll();
            return _secondLevelCache.RemoveAllAsync();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
