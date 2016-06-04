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
                throw new ObjectDisposedException("This " + nameof(MammothCache) + " object is disposed.");
            }

            SecondLevelCache.OnItemRemovedFromCache -= OnItemRemovedFromSecondLevelCache;
            _isDisposed = true;
        }

        private void SubscribeToEvents()
        {
            SecondLevelCache.OnItemRemovedFromCache += OnItemRemovedFromSecondLevelCache;
        }

        private void OnItemRemovedFromSecondLevelCache(string key)
        {
            FirstLevelCache.Remove(key);
        }

        /// <inheritdoc/>
        public T Get<T>(string key)
            where T : class
        {
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

                var ttl = SecondLevelCache.GetTimeToLive(key);
                FirstLevelCache.Set(key, bytes, ttl: ttl);

                return deserializedValue as T;
            }

            return default(T);
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key)
            where T : class
        {
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

                var ttl = await SecondLevelCache.GetTimeToLiveAsync(key).ConfigureAwait(false);
                FirstLevelCache.Set(key, bytes, ttl: ttl);

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
            if (value == null)
            {
                return;
            }

            if (SerializationProvider.CanSerialize(value.GetType()))
            {
                byte[] bytes = SerializationProvider.Serialize(value);

                FirstLevelCache.Set(key, bytes, ttl: ttl);
                SecondLevelCache.Set(key, bytes, ttl: ttl);
            }
            else
            {
                var ph = new NonSerializableObjectPlaceHolder();
                NonSerializableCache.Set(key, value, ttl: ttl);

                byte[] bytes = SerializationProvider.Serialize(ph);
                SecondLevelCache.Set(key, bytes, ttl: ttl);
            }
        }

        /// <inheritdoc/>
        public void Set<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class
        {
            var serializedValues = SaveToFirstLevelCacheAndGetSerializedValuesForSecondLevelCache(objects);
            SecondLevelCache.Set(serializedValues);
        }

        private Dictionary<CacheItemDefinition, byte[]> SaveToFirstLevelCacheAndGetSerializedValuesForSecondLevelCache<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class
        {
            var serializedValues = new Dictionary<CacheItemDefinition, byte[]>();

            foreach (var kvp in objects)
            {
                if (kvp.Value != null)
                {
                    if (SerializationProvider.CanSerialize(kvp.Value.GetType()))
                    {
                        serializedValues.Add(kvp.Key, SerializationProvider.Serialize(kvp.Value));
                        FirstLevelCache.Set(kvp.Key.Key, serializedValues[kvp.Key], ttl: kvp.Key.TimeToLive);
                    }
                    else
                    {
                        var ph = new NonSerializableObjectPlaceHolder();
                        NonSerializableCache.Set(kvp.Key.Key, kvp.Value, ttl: kvp.Key.TimeToLive);
                        byte[] bytes = SerializationProvider.Serialize(ph);
                        serializedValues.Add(kvp.Key, bytes);
                    }
                }
            }

            return serializedValues;
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

            if (SerializationProvider.CanSerialize(value.GetType()))
            {
                var bytes = SerializationProvider.Serialize(value);

                FirstLevelCache.Set(key, bytes, ttl: ttl);
                return SecondLevelCache.SetAsync(key, bytes, ttl: ttl);
            }
            else
            {
                var ph = new NonSerializableObjectPlaceHolder();
                NonSerializableCache.Set(key, value, ttl: ttl);

                byte[] bytes = SerializationProvider.Serialize(ph);
                return SecondLevelCache.SetAsync(key, bytes, ttl: ttl);
            }
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(Dictionary<CacheItemDefinition, T> objects)
            where T : class
        {
            var serializedValues = SaveToFirstLevelCacheAndGetSerializedValuesForSecondLevelCache(objects);
            return SecondLevelCache.SetAsync(serializedValues);
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            FirstLevelCache.Remove(key);
            SecondLevelCache.Remove(key);
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key)
        {
            FirstLevelCache.Remove(key);
            return SecondLevelCache.RemoveAsync(key);
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            FirstLevelCache.RemoveAll();
            SecondLevelCache.RemoveAll();
        }

        /// <inheritdoc/>
        public Task RemoveAllAsync()
        {
            FirstLevelCache.RemoveAll();
            return SecondLevelCache.RemoveAllAsync();
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

        /// <inheritdoc/>
        public Dictionary<CacheItemDefinition, T> GetOrAdd<T>(IEnumerable<CacheItemDefinition> keys,
                                                              Func<CacheItemDefinition[], Dictionary<CacheItemDefinition, T>> getAction)
            where T : class
        {
            var helper = new MultipleGetHelper(this);

            return helper.GetOrAdd<T>(keys, getAction);
        }

        /// <inheritdoc/>
        public Task<Dictionary<CacheItemDefinition, T>> GetOrAddAsync<T>(IEnumerable<CacheItemDefinition> keys,
                                                                         Func<CacheItemDefinition[], Task<Dictionary<CacheItemDefinition, T>>> getActionAsync)
            where T : class
        {
            var helper = new MultipleGetHelper(this);
            return helper.GetOrAddAsync<T>(keys, getActionAsync);
        }

        /// <inheritdoc/>
        public Dictionary<CacheItemDefinition, T> Get<T>(ICollection<CacheItemDefinition> keys)
            where T : class
        {
            var helper = new MultipleGetHelper(this);
            return helper.Get<T>(keys);
        }

        /// <inheritdoc/>
        public Task<Dictionary<CacheItemDefinition, T>> GetAsync<T>(ICollection<CacheItemDefinition> keys)
            where T : class
        {
            var helper = new MultipleGetHelper(this);
            return helper.GetAsync<T>(keys);
        }
    }
}
