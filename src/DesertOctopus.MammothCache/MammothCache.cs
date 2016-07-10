using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Distributed cache with 2 level of caching
    /// </summary>
    public sealed class MammothCache : IMammothCache, IDisposable
    {
        internal INonSerializableCache NonSerializableCache { get; private set; }

        internal IFirstLevelCache FirstLevelCache { get; private set; }

        internal ISecondLevelCache SecondLevelCache { get; private set; }

        internal IMammothCacheSerializationProvider SerializationProvider { get; private set; }

        private bool _isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MammothCache"/> class.
        /// </summary>
        /// <param name="firstLevelCache">First level of cache</param>
        /// <param name="secondLevelCache">Second level of cache</param>
        /// <param name="nonSerializableCache">Cache for objects that cannot be serialized</param>
        /// <param name="serializationProvider">Serialization provider</param>
        public MammothCache(IFirstLevelCache firstLevelCache,
                            ISecondLevelCache secondLevelCache,
                            INonSerializableCache nonSerializableCache,
                            IMammothCacheSerializationProvider serializationProvider)
        {
            NonSerializableCache = nonSerializableCache;
            FirstLevelCache = firstLevelCache;
            SecondLevelCache = secondLevelCache;
            SerializationProvider = serializationProvider;

            SubscribeToEvents();
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                GuardDisposed();
            }

            SecondLevelCache.OnItemRemovedFromCache -= OnItemRemovedFromSecondLevelCache;
            _isDisposed = true;
        }

        private void GuardDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("This " + nameof(MammothCache) + " object is disposed.");
            }
        }

        private void SubscribeToEvents()
        {
            SecondLevelCache.OnItemRemovedFromCache += OnItemRemovedFromSecondLevelCache;
            SecondLevelCache.OnRemoveAllItems += OnRemoveAllItemsFromSecondLevelCache;
        }

        private void OnRemoveAllItemsFromSecondLevelCache(object sender, RemoveAllItemsEventArgs e)
        {
            FirstLevelCache.RemoveAll();
            NonSerializableCache.RemoveAll();
        }

        private void OnItemRemovedFromSecondLevelCache(object sender, ItemEvictedEventArgs e)
        {
            FirstLevelCache.Remove(e.Key);
        }

        /// <inheritdoc/>
        public T Get<T>(string key)
            where T : class
        {
            GuardDisposed();

            var nonSerializableResult = NonSerializableCache.Get<T>(key);
            if (nonSerializableResult.IsSuccessful)
            {
                return nonSerializableResult.Value;
            }

            var firstLevelResult = FirstLevelCache.Get<T>(key);
            if (firstLevelResult.IsSuccessful)
            {
                return firstLevelResult.Value;
            }

            var bytes = SecondLevelCache.Get(key);
            if (bytes != null)
            {
                var deserializedValue = SerializationProvider.Deserialize(bytes);

                if (deserializedValue is NonSerializableObjectPlaceHolder)
                {
                    return null;
                }

                var ttlResult = SecondLevelCache.GetTimeToLive(key);
                if (ttlResult.KeyExists)
                {
                    FirstLevelCache.Set(key, bytes, ttl: ttlResult.TimeToLive);
                }

                return deserializedValue as T;
            }

            return default(T);
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key)
            where T : class
        {
            GuardDisposed();

            var nonSerializableResult = NonSerializableCache.Get<T>(key);
            if (nonSerializableResult.IsSuccessful)
            {
                return nonSerializableResult.Value;
            }

            var firstLevelResult = FirstLevelCache.Get<T>(key);
            if (firstLevelResult.IsSuccessful)
            {
                return firstLevelResult.Value;
            }

            var bytes = await SecondLevelCache.GetAsync(key).ConfigureAwait(false);
            if (bytes != null)
            {
                var deserializedValue = SerializationProvider.Deserialize(bytes);

                if (deserializedValue is NonSerializableObjectPlaceHolder)
                {
                    return null;
                }

                var ttlResult = await SecondLevelCache.GetTimeToLiveAsync(key).ConfigureAwait(false);
                if (ttlResult.KeyExists)
                {
                    FirstLevelCache.Set(key, bytes, ttl: ttlResult.TimeToLive);
                }

                return deserializedValue as T;
            }

            return default(T);
        }

        /// <inheritdoc/>
        public void Set<T>(string key,
                           T value,
                           TimeSpan? ttl = null)
            where T : class
        {
            GuardDisposed();

            if (value == null)
            {
                return;
            }

            if (SerializationProvider.CanSerialize(value.GetType()))
            {
                byte[] bytes = SerializationProvider.Serialize(value);

                SecondLevelCache.Set(key, bytes, ttl: ttl);
                FirstLevelCache.Set(key, bytes, ttl: ttl);
            }
            else
            {
                var ph = new NonSerializableObjectPlaceHolder();
                var ttlResult = SecondLevelCache.GetTimeToLive(key);

                if (ttlResult.KeyExists)
                {
                    NonSerializableCache.Set(key, value, ttl: ttlResult.TimeToLive);
                }
                else
                {
                    byte[] bytes = SerializationProvider.Serialize(ph);
                    SecondLevelCache.Set(key, bytes, ttl: ttl);
                    NonSerializableCache.Set(key, value, ttl: ttl);
                }
            }
        }

        /// <inheritdoc/>
        public void Set<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class
        {
            GuardDisposed();

            var valuesToStore = SaveToFirstLevelCacheAndGetSerializedValuesForSecondLevelCache(objects);
            if (valuesToStore.InNonSerializableCache.Count > 0)
            {
                var keys = valuesToStore.InNonSerializableCache.Select(x => x.Key.Key).ToArray();
                var ttlResults = SecondLevelCache.GetTimeToLives(keys);
                RemoveTtlResultFromValuesToStore(ttlResults, valuesToStore);
            }

            SecondLevelCache.Set(valuesToStore.InSecondLevelCache);
            StoreValuesInFirstLevelCacheAndNonSerializableCache(valuesToStore);
        }

        private void StoreValuesInFirstLevelCacheAndNonSerializableCache<T>(ValuesToStore<T> valuesToStore)
        {
            foreach (var kvp in valuesToStore.InFirstLevelCache)
            {
                FirstLevelCache.Set(kvp.Key.Key, kvp.Value, ttl: kvp.Key.TimeToLive);
            }

            foreach (var kvp in valuesToStore.InNonSerializableCache)
            {
                NonSerializableCache.Set(kvp.Key.Key, kvp.Value, ttl: kvp.Key.TimeToLive);
            }
        }

        private ValuesToStore<T> SaveToFirstLevelCacheAndGetSerializedValuesForSecondLevelCache<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class
        {
            var result = new ValuesToStore<T>();

            foreach (var kvp in objects)
            {
                if (kvp.Value != null)
                {
                    if (SerializationProvider.CanSerialize(kvp.Value.GetType()))
                    {
                        var bytes = SerializationProvider.Serialize(kvp.Value);
                        result.InSecondLevelCache.Add(kvp.Key, bytes);
                        result.InFirstLevelCache.Add(kvp.Key, bytes);
                    }
                    else
                    {
                        var ph = new NonSerializableObjectPlaceHolder();
                        result.InNonSerializableCache.Add(kvp.Key, kvp.Value);

                        byte[] bytes = SerializationProvider.Serialize(ph);
                        result.InSecondLevelCache.Add(kvp.Key, bytes);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key,
                                      T value,
                                      TimeSpan? ttl = null)
            where T : class
        {
            GuardDisposed();

            if (value == null)
            {
                return;
            }

            if (SerializationProvider.CanSerialize(value.GetType()))
            {
                var bytes = SerializationProvider.Serialize(value);

                FirstLevelCache.Set(key, bytes, ttl: ttl);
                await SecondLevelCache.SetAsync(key, bytes, ttl: ttl).ConfigureAwait(false);
            }
            else
            {
                var ph = new NonSerializableObjectPlaceHolder();
                var ttlResult = SecondLevelCache.GetTimeToLive(key);

                if (ttlResult.KeyExists)
                {
                    NonSerializableCache.Set(key, value, ttl: ttlResult.TimeToLive);
                }
                else
                {
                    byte[] bytes = SerializationProvider.Serialize(ph);
                    await SecondLevelCache.SetAsync(key, bytes, ttl: ttl).ConfigureAwait(false);
                    NonSerializableCache.Set(key, value, ttl: ttl);
                }
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class
        {
            GuardDisposed();

            var valuesToStore = SaveToFirstLevelCacheAndGetSerializedValuesForSecondLevelCache(objects);
            if (valuesToStore.InNonSerializableCache.Count > 0)
            {
                var keys = valuesToStore.InNonSerializableCache.Select(x => x.Key.Key).ToArray();
                var ttlResults = await SecondLevelCache.GetTimeToLivesAsync(keys).ConfigureAwait(false);
                RemoveTtlResultFromValuesToStore(ttlResults, valuesToStore);
            }

            await SecondLevelCache.SetAsync(valuesToStore.InSecondLevelCache).ConfigureAwait(false);
            StoreValuesInFirstLevelCacheAndNonSerializableCache(valuesToStore);
        }

        private static void RemoveTtlResultFromValuesToStore<T>(Dictionary<string, TimeToLiveResult> ttlResults,
                                                                ValuesToStore<T> valuesToStore)
            where T : class
        {
            foreach (var ttlResult in ttlResults)
            {
                if (ttlResult.Value.KeyExists)
                {
                    valuesToStore.InSecondLevelCache.Remove(valuesToStore.InSecondLevelCache.FirstOrDefault(x => x.Key.Key == ttlResult.Key)
                                                                         .Key);
                    var nonSerializableKey = valuesToStore.InNonSerializableCache.FirstOrDefault(x => x.Key.Key == ttlResult.Key);
                    nonSerializableKey.Key.TimeToLive = ttlResult.Value.TimeToLive;
                }
            }
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            GuardDisposed();
            FirstLevelCache.Remove(key);
            SecondLevelCache.Remove(key);
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key)
        {
            GuardDisposed();
            FirstLevelCache.Remove(key);
            return SecondLevelCache.RemoveAsync(key);
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            GuardDisposed();
            FirstLevelCache.RemoveAll();
            SecondLevelCache.RemoveAll();
        }

        /// <inheritdoc/>
        public Task RemoveAllAsync()
        {
            GuardDisposed();
            FirstLevelCache.RemoveAll();
            return SecondLevelCache.RemoveAllAsync();
        }

        /// <inheritdoc/>
        public T GetOrAdd<T>(string key, Func<T> getAction, TimeSpan? ttl = null)
            where T : class
        {
            GuardDisposed();

            if (getAction == null)
            {
                throw new ArgumentNullException(nameof(getAction));
            }

            var value = Get<T>(key);
            if (value != default(T))
            {
                return value;
            }

            using (var rLock = AcquireLock(key, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5)))
            {
                value = Get<T>(key);
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
        }

        /// <inheritdoc/>
        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> getActionAsync, TimeSpan? ttl = null)
            where T : class
        {
            GuardDisposed();

            if (getActionAsync == null)
            {
                throw new ArgumentNullException(nameof(getActionAsync));
            }

            var value = await GetAsync<T>(key).ConfigureAwait(false);
            if (value != default(T))
            {
                return value;
            }

            using (var rLock = AcquireLock(key, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5)))
            {
                value = await GetAsync<T>(key).ConfigureAwait(false);
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

        /// <inheritdoc/>
        public Dictionary<CacheItemDefinition, T> GetOrAdd<T>(IEnumerable<CacheItemDefinition> keys,
                                                              Func<CacheItemDefinition[], Dictionary<CacheItemDefinition, T>> getAction)
            where T : class
        {
            GuardDisposed();

            var helper = new MultipleGetHelper(this);
            return helper.GetOrAdd<T>(keys, getAction);
        }

        /// <inheritdoc/>
        public Task<Dictionary<CacheItemDefinition, T>> GetOrAddAsync<T>(IEnumerable<CacheItemDefinition> keys,
                                                                         Func<CacheItemDefinition[], Task<Dictionary<CacheItemDefinition, T>>> getActionAsync)
            where T : class
        {
            GuardDisposed();

            var helper = new MultipleGetHelper(this);
            return helper.GetOrAddAsync<T>(keys, getActionAsync);
        }

        /// <inheritdoc/>
        public Dictionary<CacheItemDefinition, T> Get<T>(ICollection<CacheItemDefinition> keys)
            where T : class
        {
            GuardDisposed();

            var helper = new MultipleGetHelper(this);
            return helper.Get<T>(keys);
        }

        /// <inheritdoc/>
        public Task<Dictionary<CacheItemDefinition, T>> GetAsync<T>(ICollection<CacheItemDefinition> keys)
            where T : class
        {
            GuardDisposed();

            var helper = new MultipleGetHelper(this);
            return helper.GetAsync<T>(keys);
        }

        /// <inheritdoc/>
        public IDisposable AcquireLock(string key, TimeSpan lockExpiry, TimeSpan timeout)
        {
            GuardDisposed();
            return SecondLevelCache.AcquireLock(key, lockExpiry, timeout);
        }

        /// <inheritdoc/>
        public Task<IDisposable> AcquireLockAsync(string key, TimeSpan lockExpiry, TimeSpan timeout)
        {
            GuardDisposed();
            return SecondLevelCache.AcquireLockAsync(key, lockExpiry, timeout);
        }
    }
}