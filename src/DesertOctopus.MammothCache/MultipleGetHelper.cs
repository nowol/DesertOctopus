using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    internal class MultipleGetHelper
    {
        private MammothCache _mammothCache;

        public MultipleGetHelper(MammothCache mammothCache)
        {
            _mammothCache = mammothCache;
        }

        private void AddToDictionary<T>(Dictionary<CacheItemDefinition, T> source, Dictionary<CacheItemDefinition, T> destination)
        {
            foreach (var kvp in source)
            {
                destination.Add(kvp.Key, kvp.Value);
            }
        }

        private void AddItemsToFirstLevelCache<T>(Dictionary<CacheItemDefinition, T> items)
            where T : class
        {
            foreach (var kvp in items)
            {
                if (kvp.Value != null)
                {
                    if (_mammothCache.SerializationProvider.CanSerialize(kvp.Value.GetType()))
                    {
                        _mammothCache.FirstLevelCache.Set(kvp.Key.Key, _mammothCache.SerializationProvider.Serialize(kvp.Value), ttl: kvp.Key.TimeToLive);
                    }
                    else
                    {
                        _mammothCache.NonSerializableCache.Set(kvp.Key.Key, kvp.Value, ttl: kvp.Key.TimeToLive);
                    }
                }
            }
        }

        private CacheItemDefinition[] DetectMissingItems<T>(CacheItemDefinition[] arrayKeys,
                                                            Dictionary<CacheItemDefinition, T> result)
        {
            return arrayKeys.Where(x => result.All(y => y.Key.Key != x.Key))
                            .ToArray();
        }

        public Dictionary<CacheItemDefinition, T> GetOrAdd<T>(IEnumerable<CacheItemDefinition> keys,
                                                              Func<CacheItemDefinition[], Dictionary<CacheItemDefinition, T>> getAction)
            where T : class
        {
            var arrayKeys = keys.ToArray();
            var result = Get<T>(arrayKeys);

            var itemsStillMissingFromCache = DetectMissingItems(arrayKeys, result);
            if (itemsStillMissingFromCache.Length > 0)
            {
                var itemsFromAction = getAction(itemsStillMissingFromCache);
                _mammothCache.Set(itemsFromAction);
                AddToDictionary(itemsFromAction, result);
                AddItemsToFirstLevelCache(itemsFromAction);
            }

            return result;
        }

        public async Task<Dictionary<CacheItemDefinition, T>> GetOrAddAsync<T>(IEnumerable<CacheItemDefinition> keys,
                                                                               Func<CacheItemDefinition[], Task<Dictionary<CacheItemDefinition, T>>> getActionAsync)
            where T : class
        {
            var arrayKeys = keys.ToArray();

            var result = await GetAsync<T>(arrayKeys).ConfigureAwait(false);

            var itemsStillMissingFromCache = DetectMissingItems(arrayKeys, result);
            if (itemsStillMissingFromCache.Length > 0)
            {
                var itemsFromAction = await getActionAsync(itemsStillMissingFromCache).ConfigureAwait(false);
                await _mammothCache.SetAsync(itemsFromAction).ConfigureAwait(false);
                AddToDictionary(itemsFromAction, result);
                AddItemsToFirstLevelCache(itemsFromAction);
            }

            return result;
        }

        private KeysAnalyzerFromFirstLevelCache<T> GetItemsFromFirstLevelCache<T>(ICollection<CacheItemDefinition> keys)
            where T : class
        {
            var result = new KeysAnalyzerFromFirstLevelCache<T>();

            foreach (var cacheItemDefinition in keys)
            {
                var cachedItemResult = _mammothCache.NonSerializableCache.Get<T>(cacheItemDefinition.Key);
                if (cachedItemResult.IsSuccessful)
                {
                    result.ItemsFromFirstLevelCache.Add(cacheItemDefinition, cachedItemResult.Value);
                }
                else
                {
                    cachedItemResult = _mammothCache.FirstLevelCache.Get<T>(cacheItemDefinition.Key);
                    if (cachedItemResult.IsSuccessful)
                    {
                        result.ItemsFromFirstLevelCache.Add(cacheItemDefinition, cachedItemResult.Value);
                    }
                    else
                    {
                        result.ItemsToGetFromSecondLevelCache.Add(cacheItemDefinition);
                    }
                }
            }

            return result;
        }

        private class KeysAnalyzerFromFirstLevelCache<T>
            where T : class
        {
            public Dictionary<CacheItemDefinition, T> ItemsFromFirstLevelCache { get; set; }

            public List<CacheItemDefinition> ItemsToGetFromSecondLevelCache { get; set; }

            public KeysAnalyzerFromFirstLevelCache()
            {
                ItemsFromFirstLevelCache = new Dictionary<CacheItemDefinition, T>();
                ItemsToGetFromSecondLevelCache = new List<CacheItemDefinition>();
            }
        }

        public Dictionary<CacheItemDefinition, T> Get<T>(ICollection<CacheItemDefinition> keys)
            where T : class
        {
            var firstLevelCachedItem = GetItemsFromFirstLevelCache<T>(keys);
            var itemsFromSecondLevelCache = _mammothCache.SecondLevelCache.Get(firstLevelCachedItem.ItemsToGetFromSecondLevelCache);
            var result = ConvertItemsFromSecondLevelCacheToDeserializedDictionary<T>(itemsFromSecondLevelCache);
            AddToDictionary(firstLevelCachedItem.ItemsFromFirstLevelCache, result);

            return result;
        }

        public async Task<Dictionary<CacheItemDefinition, T>> GetAsync<T>(ICollection<CacheItemDefinition> keys)
            where T : class
        {
            var firstLevelCachedItem = GetItemsFromFirstLevelCache<T>(keys);
            var itemsFromSecondLevelCache = await _mammothCache.SecondLevelCache.GetAsync(firstLevelCachedItem.ItemsToGetFromSecondLevelCache).ConfigureAwait(false);
            var result = ConvertItemsFromSecondLevelCacheToDeserializedDictionary<T>(itemsFromSecondLevelCache);
            AddToDictionary(firstLevelCachedItem.ItemsFromFirstLevelCache, result);

            return result;
        }

        private Dictionary<CacheItemDefinition, T> ConvertItemsFromSecondLevelCacheToDeserializedDictionary<T>(Dictionary<CacheItemDefinition, byte[]> itemsFromSecondLevelCache)
            where T : class
        {
            var result = new Dictionary<CacheItemDefinition, T>();

            foreach (var itemFromSll in itemsFromSecondLevelCache)
            {
                if (itemFromSll.Value != null)
                {
                    var value = _mammothCache.SerializationProvider.Deserialize(itemFromSll.Value);

                    if (!(value is NonSerializableObjectPlaceHolder))
                    {
                        result.Add(itemFromSll.Key, value as T);
                        _mammothCache.FirstLevelCache.Set(itemFromSll.Key.Key, itemFromSll.Value, ttl: itemFromSll.Key.TimeToLive);
                    }
                }
            }

            return result;
        }
    }
}
