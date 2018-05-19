using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis;
using DesertOctopus.MammothCache.Tests.Models;
using Xunit;

namespace DesertOctopus.MammothCache.Tests
{
    public class MammothCacheIntegrationTest : BaseTest, IDisposable
    {
        private SquirrelCache _firstLevelCache;
        private CachingTestClass _testObject;
        private CachingTestClass _testObject2;
        private CachingTestClass _testObject3;
        private byte[] _serializedTestObject;
        private readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private readonly IFirstLevelCacheCloningProvider _noCloningProvider = new NoCloningProvider();
        private readonly NonSerializableCache _nonSerializableCache = new NonSerializableCache();
        private readonly NotSerializableTestClass _nonSerializableTestObject = new NotSerializableTestClass();
        private readonly NotSerializableTestClass _nonSerializableTestObject2 = new NotSerializableTestClass();
        private readonly IMammothCacheSerializationProvider _serializationProvider = new MammothCacheSerializationProvider();

        private RedisConnection _secondLevelCache;
        private IRedisRetryPolicy _redisRetryPolicy;

        private MammothCache _cache;
        private MammothCacheSerializationProvider _mammothCacheSerializationProvider;

        public MammothCacheIntegrationTest()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(10);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = TimeSpan.FromSeconds(60);

            _firstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            _testObject = new CachingTestClass();
            _testObject2 = new CachingTestClass();
            _testObject3 = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _secondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);

            _mammothCacheSerializationProvider = new MammothCacheSerializationProvider();
            _cache = new MammothCache(_firstLevelCache, _secondLevelCache, _nonSerializableCache, _mammothCacheSerializationProvider);

            RemoveAllAndWait();

#if !DEBUG
            WaitFor(0.5);
#endif
        }

        public void Dispose()
        {
            try
            {
                RemoveAllAndWait();
            }
            catch
            {
                // dont really care if it fails
            }

            try
            {
                _firstLevelCache.Dispose();
            }
            catch
            {
                // dont really care if it fails
            }

            try
            {
                _secondLevelCache.Dispose();
            }
            catch
            {
                // dont really care if it fails
            }


            try
            {
                _cache.Dispose();
            }
            catch
            {
                // dont really care if it fails
            }

            _nonSerializableCache.Dispose();
        }

        private void RemoveAllAndWait()
        {
            var receivedEvent = false;

            _secondLevelCache.OnRemoveAllItems += (sender,
                                                   args) =>
                                                  {
                                                      receivedEvent = true;
                                                  };

            _cache.RemoveAll();

            var sw = Stopwatch.StartNew();
            while (!receivedEvent && sw.Elapsed < TimeSpan.FromSeconds(30))
            {
                WaitFor(0.01);
            }
        }

        private async Task RemoveAllAndWaitAsync()
        {
            var receivedEvent = false;

            _secondLevelCache.OnRemoveAllItems += (sender,
                                                   args) =>
                                                  {
                                                      receivedEvent = true;
                                                  };

            await _cache.RemoveAllAsync().ConfigureAwait(false);

            var sw = Stopwatch.StartNew();
            while (!receivedEvent && sw.Elapsed < TimeSpan.FromSeconds(30))
            {
                WaitFor(0.01);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AddingAnItemShouldAddItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AddingAnItemShouldAddItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RemovingAnItemShouldRemoveItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            await _cache.RemoveAsync(key).ConfigureAwait(false);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemovingAnItemShouldRemoveItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            _cache.Remove(key);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdatingAnItemShouldRemoveItFromOtherDistributedCacheAsync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemoved = true;
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemovedFromOtherCache = true;

            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            await otherCache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.True(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void UpdatingAnItemShouldRemoveItFromOtherDistributedCacheSync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemoved = true;
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemovedFromOtherCache = true;

            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            otherCache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.True(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdatingMultipleItemsShouldRemoveItFromOtherDistributedCacheAsync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var objects = new Dictionary<CacheItemDefinition, CachingTestClass>();
            objects.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            objects.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject2);
            objects.Add(new CacheItemDefinition { Key = key3 }, _testObject3);

            bool itemsWereRemoved = false;
            int itemsWereRemovedCount = 0;
            bool itemsWereRemovedFromOtherCache = false;
            int itemsWereRemovedCountFromOtherCache = 0;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };

            await _cache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);
            Assert.Equal(0, itemsWereRemovedCount);
            Assert.Equal(3, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            await otherCache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.True(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
            Assert.Equal(3, itemsWereRemovedCount);
            Assert.Equal(0, itemsWereRemovedCountFromOtherCache);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void UpdatingMultipleItemsShouldRemoveItFromOtherDistributedCacheSync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var objects = new Dictionary<CacheItemDefinition, CachingTestClass>();
            objects.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            objects.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject2);
            objects.Add(new CacheItemDefinition { Key = key3 }, _testObject3);

            bool itemsWereRemoved = false;
            int itemsWereRemovedCount = 0;
            bool itemsWereRemovedFromOtherCache = false;
            int itemsWereRemovedCountFromOtherCache = 0;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };

            _cache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);
            Assert.Equal(0, itemsWereRemovedCount);
            Assert.Equal(3, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            otherCache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.True(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
            Assert.Equal(3, itemsWereRemovedCount);
            Assert.Equal(0, itemsWereRemovedCountFromOtherCache);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdatingAnItemShouldUpdateItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            var testObject2 = new CachingTestClass();
            await _cache.SetAsync(key, testObject2, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.Equal(testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            bytes = _secondLevelCache.Get(key);
            Assert.Equal(testObject2.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void UpdatingAnItemShouldUpdateItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            var testObject2 = new CachingTestClass();
            _cache.Set(key, testObject2, TimeSpan.FromSeconds(30));

            Assert.Equal(testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            bytes = _secondLevelCache.Get(key);
            Assert.Equal(testObject2.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ExpiredItemFromFirstLevelShouldStillExistInSecondLevelAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.NotNull(bytes);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ExpiredItemFromFirstLevelShouldStillExistInSecondLevelSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.NotNull(bytes);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ItemShouldBePutIntoFirstLevelCacheWhenFetchFromTheSecondLevelCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            Assert.NotNull(await _cache.GetAsync<CachingTestClass>(key).ConfigureAwait(false));

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ItemShouldBePutIntoFirstLevelCacheWhenFetchFromTheSecondLevelCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            Assert.NotNull(_cache.Get<CachingTestClass>(key));

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RemovingAnItemFromTheSecondLevelCacheShouldRemoveItFromTheFirstLevelCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            _secondLevelCache.Remove(key);
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemovingAnItemFromTheSecondLevelCacheShouldRemoveItFromTheFirstLevelCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            _secondLevelCache.Remove(key);
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ItemIsRemovedFromFirstLevelCacheIfItExpiresFromSecondLevelCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            _secondLevelCache.Set(key, _serializedTestObject, TimeSpan.FromMilliseconds(200));
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ItemIsRemovedFromFirstLevelCacheIfItExpiresFromSecondLevelCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            _secondLevelCache.Set(key, _serializedTestObject, TimeSpan.FromMilliseconds(200));
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ItemsEvictedFromBecauseOfMemoryPressureShouldBeRemovedFromFirstLevelCacheAsync()
        {
            var c = _secondLevelCache.GetConfig(null);
            var config = await _secondLevelCache.GetConfigAsync(pattern: "maxmemory").ConfigureAwait(false);
            var memoryStr = RedisMaxMemory;
            if (config != null
                && config.Length == 1)
            {
                memoryStr = config[0].Value;
            }
            int memoryLimit;
            if (!int.TryParse(memoryStr, out memoryLimit)
                && memoryLimit > 0)
            {
                throw new NotSupportedException("Could not parse maxmemory: " + memoryStr);
            }

            int slicedInto = 1000;
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[memoryLimit / slicedInto] };
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);
            var nbToStore = (memoryLimit / bigSerializedTestObject.Length) + 10;
            var keys = new List<string>();
            bool itemsWereRemoved = true;
            var removedKeys = new ConcurrentBag<string>();

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                        {
                                                            Assert.Contains(e.Key,
                                                                            keys);
                                                            itemsWereRemoved = true;
                                                            removedKeys.Add(e.Key);
                                                        };

            var tasks = new List<Task>();

            for (int i = 0; i < nbToStore; i++)
            {
                var key = RandomKey();
                keys.Add(key);
                var task = _cache.SetAsync(key, bigSerializedTestObject, TimeSpan.FromSeconds(300));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);

            WaitFor(20);

            Assert.True(itemsWereRemoved);

            foreach (var key in removedKeys.Take(10).ToArray())
            {
                Assert.False(_firstLevelCache.Get<object>(key).IsSuccessful);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetOrAddShouldUseTheItemProvidedByTheDelegateIfItIsMissingFromTheCacheAsync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var value = await _cache.GetOrAddAsync<CachingTestClass>(key,
                                                                     () =>
                                                                     {
                                                                         delegateWasCalled = true;
                                                                         return Task.FromResult(_testObject);
                                                                     },
                                                                     TimeSpan.FromSeconds(30))
                                    .ConfigureAwait(false);
            Assert.True(delegateWasCalled);
            Assert.Equal(_testObject.Value, value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);

            var bytes = await _secondLevelCache.GetAsync(key).ConfigureAwait(false);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.Equal(_testObject.Value, deserializedValue.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetOrAddShouldUseTheItemProvidedByTheDelegateIfItIsMissingFromTheCacheSync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var value = _cache.GetOrAdd<CachingTestClass>(key,
                                                          () =>
                                                          {
                                                              delegateWasCalled = true;
                                                              return _testObject;
                                                          },
                                                          TimeSpan.FromSeconds(30));
            Assert.True(delegateWasCalled);
            Assert.Equal(_testObject.Value, value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);

            var bytes = _secondLevelCache.Get(key);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.Equal(_testObject.Value, deserializedValue.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetOrAddNotShouldUseTheDelegateIfTheItemIsAlreadyCachedAsync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            _firstLevelCache.RemoveAll();

            var value = await _cache.GetOrAddAsync<CachingTestClass>(key,
                                                                     () =>
                                                                     {
                                                                         delegateWasCalled = true;
                                                                         return Task.FromResult(_testObject);
                                                                     },
                                                                     TimeSpan.FromSeconds(30))
                                    .ConfigureAwait(false);
            Assert.False(delegateWasCalled);
            Assert.Equal(1, _firstLevelCache.NumberOfObjects);
            Assert.Equal(_testObject.Value, value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetOrAddNotShouldUseTheDelegateIfTheItemIsAlreadyCachedSync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));
            _firstLevelCache.RemoveAll();

            var value = _cache.GetOrAdd<CachingTestClass>(key,
                                                          () =>
                                                          {
                                                              delegateWasCalled = true;
                                                              return _testObject;
                                                          },
                                                          TimeSpan.FromSeconds(30));
            Assert.False(delegateWasCalled);
            Assert.Equal(1, _firstLevelCache.NumberOfObjects);
            Assert.Equal(_testObject.Value, value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetOrAddMultipleItemsShouldUseTheItemProvidedByTheDelegateIfItIsMissingFromTheCacheAsync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = await _cache.GetOrAddAsync<CachingTestClass>(keys,
                                                                      definitions =>
                                                                      {
                                                                          delegateWasCalled = true;
                                                                          var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                                          results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                                          return Task.FromResult(results);
                                                                      })
                                     .ConfigureAwait(false);
            Assert.True(delegateWasCalled);
            Assert.Single(values);
            Assert.Equal(_testObject.Value, values.First().Value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.False(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.False(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);

            var bytes = await _secondLevelCache.GetAsync(key1).ConfigureAwait(false);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.Equal(_testObject.Value, deserializedValue.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetOrAddMultipleItemsShouldUseTheItemProvidedByTheDelegateIfItIsMissingFromTheCacheSync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = _cache.GetOrAdd<CachingTestClass>(keys,
                                                           definitions =>
                                                           {
                                                               delegateWasCalled = true;
                                                               var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                               results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                               return results;
                                                           });
            Assert.True(delegateWasCalled);
            Assert.Single(values);
            Assert.Equal(_testObject.Value, values.First().Value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.False(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.False(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);

            var bytes = _secondLevelCache.Get(key1);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.Equal(_testObject.Value, deserializedValue.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task SetMultipleItemsShouldStoreAllItemsAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var objects = new Dictionary<CacheItemDefinition, CachingTestClass>();
            objects.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            objects.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) }, _testObject2);
            objects.Add(new CacheItemDefinition { Key = key3 }, _testObject3);

            await _cache.SetAsync(objects).ConfigureAwait(false);

            Assert.Equal(3, _firstLevelCache.NumberOfObjects);
            Assert.True(await _secondLevelCache.KeyExistsAsync(key1).ConfigureAwait(false));
            Assert.True(await _secondLevelCache.KeyExistsAsync(key1).ConfigureAwait(false));
            Assert.True(await _secondLevelCache.KeyExistsAsync(key1).ConfigureAwait(false));

            Assert.True(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.True(_secondLevelCache.GetTimeToLive(key2).TimeToLive.Value.TotalSeconds > 5);
            Assert.False(_secondLevelCache.GetTimeToLive(key3).TimeToLive.HasValue);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetMultipleItemsShouldStoreAllItemsSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var objects = new Dictionary<CacheItemDefinition, CachingTestClass>();
            objects.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            objects.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) }, _testObject2);
            objects.Add(new CacheItemDefinition { Key = key3 }, _testObject3);

            _cache.Set(objects);

            Assert.Equal(3, _firstLevelCache.NumberOfObjects);
            Assert.True(_secondLevelCache.KeyExists(key1));
            Assert.True(_secondLevelCache.KeyExists(key1));
            Assert.True(_secondLevelCache.KeyExists(key1));

            Assert.True(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.True(_secondLevelCache.GetTimeToLive(key2).TimeToLive.Value.TotalSeconds > 5);
            Assert.False(_secondLevelCache.GetTimeToLive(key3).TimeToLive.HasValue);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetOrAddMultipleItemsNotShouldUseTheDelegateIfTheItemIsAlreadyCachedAsync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            await _cache.SetAsync(key1, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            await _cache.SetAsync(key2, _testObject2, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            await _cache.SetAsync(key3, _testObject3, TimeSpan.FromSeconds(30)).ConfigureAwait(false);


            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = await _cache.GetOrAddAsync<CachingTestClass>(keys,
                                                                      definitions =>
                                                                      {
                                                                          delegateWasCalled = true;
                                                                          var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                                          results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                                          return Task.FromResult(results);
                                                                      })
                                     .ConfigureAwait(false);
            Assert.False(delegateWasCalled);
            Assert.Equal(3, values.Count);
            Assert.Equal(_testObject.Value, values.First().Value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.Equal(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.Equal(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetOrAddMultipleItemsNotShouldUseTheDelegateIfTheItemIsAlreadyCachedSync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            _cache.Set(key1, _testObject, TimeSpan.FromSeconds(30));
            _cache.Set(key2, _testObject2, TimeSpan.FromSeconds(30));
            _cache.Set(key3, _testObject3, TimeSpan.FromSeconds(30));


            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = _cache.GetOrAdd<CachingTestClass>(keys,
                                                           definitions =>
                                                           {
                                                               delegateWasCalled = true;
                                                               var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                               results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                               return results;
                                                           });
            Assert.False(delegateWasCalled);
            Assert.Equal(3, values.Count);
            Assert.Equal(_testObject.Value, values.First().Value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.Equal(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.Equal(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetOrAddMultipleItemsNotShouldOnlyUseTheDelegateIfSomeItemsAreMissingFromTheCacheAsync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            await _cache.SetAsync(key1, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            await _cache.SetAsync(key2, _testObject2, TimeSpan.FromSeconds(30)).ConfigureAwait(false);


            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = await _cache.GetOrAddAsync<CachingTestClass>(keys,
                                                                      definitions =>
                                                                      {
                                                                          Assert.Single(definitions);
                                                                          delegateWasCalled = true;
                                                                          var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                                          results.Add(definitions.Single(x => x.Key == key3), _testObject3);
                                                                          return Task.FromResult(results);
                                                                      })
                                     .ConfigureAwait(false);
            Assert.True(delegateWasCalled);
            Assert.Equal(3, values.Count);
            Assert.Equal(_testObject.Value, values.First().Value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.Equal(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.Equal(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetOrAddMultipleItemsNotShouldOnlyUseTheDelegateIfSomeItemsAreMissingFromTheCacheSync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            _cache.Set(key1, _testObject, TimeSpan.FromSeconds(30));
            _cache.Set(key2, _testObject2, TimeSpan.FromSeconds(30));

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = _cache.GetOrAdd<CachingTestClass>(keys,
                                                           definitions =>
                                                           {
                                                               Assert.Single(definitions);
                                                               delegateWasCalled = true;
                                                               var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                               results.Add(definitions.Single(x => x.Key == key3), _testObject3);
                                                               return results;
                                                           });
            Assert.True(delegateWasCalled);
            Assert.Equal(3, values.Count);
            Assert.Equal(_testObject.Value, values.First().Value.Value);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.True(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.Equal(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.Equal(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetMultipleValuesAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();
            var testObject2 = new CachingTestClass();

            _cache.Set(key1, _testObject, TimeSpan.FromSeconds(30));
            _cache.Set(key2, testObject2, TimeSpan.FromSeconds(30));

            _firstLevelCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });
            keys.Add(new CacheItemDefinition { Key = key3 });

            Dictionary<CacheItemDefinition, CachingTestClass> values = await _cache.GetAsync<CachingTestClass>(keys).ConfigureAwait(false);

            Assert.Equal(2, values.Count);
            Assert.Equal(2, _firstLevelCache.NumberOfObjects);
            Assert.Equal(_testObject.Value, values.First(x => x.Key.Key == key1).Value.Value);
            Assert.Equal(testObject2.Value, values.First(x => x.Key.Key == key2).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetMultipleValuesSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();
            var testObject2 = new CachingTestClass();

            _cache.Set(key1, _testObject, TimeSpan.FromSeconds(30));
            _cache.Set(key2, testObject2, TimeSpan.FromSeconds(30));

            _firstLevelCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });
            keys.Add(new CacheItemDefinition { Key = key3 });

            Dictionary<CacheItemDefinition, CachingTestClass> values = _cache.Get<CachingTestClass>(keys);

            Assert.Equal(2, values.Count);
            Assert.Equal(2, _firstLevelCache.NumberOfObjects);
            Assert.Equal(_testObject.Value, values.First(x => x.Key.Key == key1).Value.Value);
            Assert.Equal(testObject2.Value, values.First(x => x.Key.Key == key2).Value.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task SetMultipleValuesAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            await _cache.SetAsync(values).ConfigureAwait(false);
            Assert.Equal(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();
            Assert.Equal(_testObject.Value, _cache.Get<CachingTestClass>(key1).Value);
            Assert.Equal(testObject2.Value, _cache.Get<CachingTestClass>(key2).Value);

            Assert.True(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.Null(_secondLevelCache.GetTimeToLive(key2).TimeToLive);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetMultipleValuesSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            _cache.Set(values);

            Assert.Equal(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();
            Assert.Equal(_testObject.Value, _cache.Get<CachingTestClass>(key1).Value);
            Assert.Equal(testObject2.Value, _cache.Get<CachingTestClass>(key2).Value);

            Assert.True(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.Null(_secondLevelCache.GetTimeToLive(key2).TimeToLive);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task MultipleItemsRetrievedShouldHaveTheirTtlSetAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(5) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            await _cache.SetAsync(values).ConfigureAwait(false);
            Assert.Equal(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            values = await _cache.GetAsync<CachingTestClass>(keys).ConfigureAwait(false);
            _secondLevelCache.Set(key1, _serializedTestObject, TimeSpan.FromSeconds(30));

            WaitFor(5);
            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.Equal(1, _firstLevelCache.NumberOfObjects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds);

            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void MultipleItemsRetrievedShouldHaveTheirTtlSetSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(5) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            _cache.Set(values);
            Assert.Equal(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            values = _cache.Get<CachingTestClass>(keys);
            _secondLevelCache.Set(key1, _serializedTestObject, TimeSpan.FromSeconds(30));

            WaitFor(5);

            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.Equal(1, _firstLevelCache.NumberOfObjects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds);

            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemovingAnItemShouldRemoveItFromAllMammothCaches()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));
            _cache.Get<CachingTestClass>(key);
            otherCache.Get<CachingTestClass>(key);

            Assert.Equal(1, _firstLevelCache.NumberOfObjects);
            Assert.Equal(1, otherFirstLevelCache.NumberOfObjects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            Assert.Equal(0, otherFirstLevelCache.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void UpdatingAnItemShouldRemoveItFromAllFirstLevelCaches()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));
            _cache.Get<CachingTestClass>(key);
            otherCache.Get<CachingTestClass>(key);

            Assert.Equal(1, _firstLevelCache.NumberOfObjects);
            Assert.Equal(1, otherFirstLevelCache.NumberOfObjects);

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            Assert.Equal(0, otherFirstLevelCache.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RemoveAllShouldEmptyTheCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            await RemoveAllAndWaitAsync().ConfigureAwait(false);

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemoveAllShouldEmptyTheCache()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.Equal(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.Equal(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            RemoveAllAndWait();

            Assert.False(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public Task UsingGetOrAddSingleItemWithoutDelegateShouldThrowAnExceptionAsync()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => _cache.GetOrAddAsync<CachingTestClass>(RandomKey(), null));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void UsingGetOrAddSingleItemWithoutDelegateShouldThrowAnExceptionSync()
        {
            Assert.Throws<ArgumentNullException>(() => _cache.GetOrAdd<CachingTestClass>(RandomKey(), null));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ItemIsFetchedFromFirstLevelCacheAsync()
        {
            var key = RandomKey();
            _firstLevelCache.Set(key, _serializedTestObject);

            var obj = await _cache.GetAsync<CachingTestClass>(key).ConfigureAwait(false);
            Assert.Equal(_testObject.Value, obj.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ItemIsFetchedFromFirstLevelCacheSync()
        {
            var key = RandomKey();
            _firstLevelCache.Set(key, _serializedTestObject);

            var obj = _cache.Get<CachingTestClass>(key);
            Assert.Equal(_testObject.Value, obj.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CachingANullObjectDoesNothingAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync<CachingTestClass>(key, null).ConfigureAwait(false);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CachingANullObjectDoesNothingSync()
        {
            var key = RandomKey();
            _cache.Set<CachingTestClass>(key, null);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            Assert.Null(_secondLevelCache.Get(key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void DeserializeObject()
        {
            var obj = _mammothCacheSerializationProvider.Deserialize(_serializedTestObject) as CachingTestClass;
            Assert.Equal(_testObject.Value, obj.Value);
        }





        [Fact]
        [Trait("Category", "Integration")]
        public async Task CachingANonSerializableObjectShouldStoreItInNonSerializableCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);

            Assert.Equal(1, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CachingANonSerializableObjectShouldStoreItInNonSerializableCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);

            Assert.Equal(1, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CachingMultipleNonSerializableObjectShouldStoreItInNonSerializableCacheAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);

            await _cache.SetAsync(values).ConfigureAwait(false);

            Assert.Equal(2, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            foreach (var key in new [] { key1, key2 })
            {
                var bytes = _secondLevelCache.Get(key);
                Assert.NotNull(bytes);
                var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
                Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CachingMultipleNonSerializableObjectShouldStoreItInNonSerializableCacheSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);

            _cache.Set(values);

            Assert.Equal(2, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            foreach (var key in new [] { key1, key2 })
            {
                var bytes = _secondLevelCache.Get(key);
                Assert.NotNull(bytes);
                var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
                Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RetrievingMultipleNonSerializableObjectShouldReturnTheSameReferenceAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);

            await _cache.SetAsync(values).ConfigureAwait(false);

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            var objs = await _cache.GetAsync<NotSerializableTestClass>(keys).ConfigureAwait(false);
            Assert.True(ReferenceEquals(_nonSerializableTestObject, objs.Single(x => x.Value.Value == _nonSerializableTestObject.Value).Value));
            Assert.True(ReferenceEquals(_nonSerializableTestObject2, objs.Single(x => x.Value.Value == _nonSerializableTestObject2.Value).Value));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RetrievingMultipleNonSerializableObjectShouldReturnTheSameReferenceSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);

            _cache.Set(values);

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            var objs = _cache.Get<NotSerializableTestClass>(keys);
            Assert.True(ReferenceEquals(_nonSerializableTestObject, objs.Single(x => x.Value.Value == _nonSerializableTestObject.Value).Value));
            Assert.True(ReferenceEquals(_nonSerializableTestObject2, objs.Single(x => x.Value.Value == _nonSerializableTestObject2.Value).Value));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RetrievinMultipleANonSerializableObjectShouldReturnTheSameReferenceAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);
            var obj = await _cache.GetAsync<NotSerializableTestClass>(key).ConfigureAwait(false);
            Assert.True(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RetrievinMultipleANonSerializableObjectShouldReturnTheSameReferenceSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);
            var obj = _cache.Get<NotSerializableTestClass>(key);
            Assert.True(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RetrievingANonSerializableObjectShouldReturnTheSameReferenceAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);
            var obj = await _cache.GetAsync<NotSerializableTestClass>(key).ConfigureAwait(false);
            Assert.True(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RetrievingANonSerializableObjectShouldReturnTheSameReferenceSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);
            var obj = _cache.Get<NotSerializableTestClass>(key);
            Assert.True(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RetrievingANonSerializableObjectFromSecondLevelCacheShouldReturnNullAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);
            _nonSerializableCache.RemoveAll();
            Assert.Null(await _cache.GetAsync<NotSerializableTestClass>(key).ConfigureAwait(false));
            Assert.Equal(0, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RetrievingANonSerializableObjectFromSecondLevelCacheShouldReturnNullSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);
            _nonSerializableCache.RemoveAll();
            Assert.Null(_cache.Get<NotSerializableTestClass>(key));
            Assert.Equal(0, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RetrievingMultipleNonSerializableObjectFromSecondLevelCacheShouldReturnNullAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);

            await _cache.SetAsync(values).ConfigureAwait(false);
            _nonSerializableCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            var objs = await _cache.GetAsync<NotSerializableTestClass>(keys).ConfigureAwait(false);
            Assert.Empty(objs);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RetrievingMultipleNonSerializableObjectFromSecondLevelCacheShouldReturnNullSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);

            _cache.Set(values);
            _nonSerializableCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            var objs = _cache.Get<NotSerializableTestClass>(keys);
            Assert.Empty(objs);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemovingANonSerializableObjectPlaceHolderFromSecondLevelCacheShouldRemoveItFromTheNonSerializableCache()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(5));
            Assert.True(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ANonSerializableSystemTypeShouldBeStoredInTheNonSerializableCache()
        {
            using (var ms = new MemoryStream())
            {
                var obj = new System.IO.BinaryReader(ms);

                var key = RandomKey();
                _cache.Set(key, obj);
                Assert.Null(_cache.Get<NotSerializableTestClass>(key));
                Assert.Equal(1, _nonSerializableCache.NumberOfObjects);
                Assert.Equal(0, _firstLevelCache.NumberOfObjects);
                var bytes = _secondLevelCache.Get(key);
                Assert.NotNull(bytes);
                var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
                Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ANonSerializableObjectShouldBeReturnedByGetOrAddAsync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var value = await _cache.GetOrAddAsync<NotSerializableTestClass>(key,
                                                                             () =>
                                                                             {
                                                                                 delegateWasCalled = true;
                                                                                 return Task.FromResult(_nonSerializableTestObject);
                                                                             },
                                                                             TimeSpan.FromSeconds(30))
                                    .ConfigureAwait(false);
            Assert.True(delegateWasCalled);
            Assert.Equal(_nonSerializableTestObject.Value, value.Value);
            Assert.True(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            Assert.False(_firstLevelCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ANonSerializableObjectShouldBeReturnedByGetOrAddSync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var value = _cache.GetOrAdd<NotSerializableTestClass>(key,
                                                                  () =>
                                                                  {
                                                                      delegateWasCalled = true;
                                                                      return _nonSerializableTestObject;
                                                                  },
                                                                  TimeSpan.FromSeconds(30));
            Assert.True(delegateWasCalled);
            Assert.Equal(_nonSerializableTestObject.Value, value.Value);
            Assert.True(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            Assert.False(_firstLevelCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task MultipleNonSerializableObjectsShouldBeReturnedByGetOrAddAsync()
        {
#pragma warning disable 219
            bool delegateWasCalled = false;
#pragma warning restore 219
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30), Cargo = 3});
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = await _cache.GetOrAddAsync<NotSerializableTestClass>(keys,
                                                                              definitions =>
                                                                              {
                                                                                  delegateWasCalled = true;
                                                                                  var results = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
                                                                                  results.Add(definitions.Single(x => x.Cargo != null && (int)x.Cargo == 3), _nonSerializableTestObject);

                                                                                  return Task.FromResult(results);
                                                                              })
                                     .ConfigureAwait(false);

            Assert.Single(values);
            Assert.Equal(1, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key1);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);

            Assert.Null(_secondLevelCache.Get(key2));
            Assert.Null(_secondLevelCache.Get(key3));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void MultipleNonSerializableObjectsShouldBeReturnedByGetOrAddSync()
        {
#pragma warning disable 219
            bool delegateWasCalled = false;
#pragma warning restore 219
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = _cache.GetOrAdd<NotSerializableTestClass>(keys,
                                                                   definitions =>
                                                                   {
                                                                       delegateWasCalled = true;
                                                                       var results = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
                                                                       results.Add(definitions.Single(x => x.Key == key1), _nonSerializableTestObject);

                                                                       return results;
                                                                   });

            Assert.Single(values);
            Assert.Equal(1, _nonSerializableCache.NumberOfObjects);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key1);
            Assert.NotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.True(deserializedValue is NonSerializableObjectPlaceHolder);

            Assert.Null(_secondLevelCache.Get(key2));
            Assert.Null(_secondLevelCache.Get(key3));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AcquiringAndReleasingALockShouldCreateTheKeyInRedisAsync()
        {
            var key = RandomKey();
            using (await _cache.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Assert.True(await _secondLevelCache.KeyExistsAsync("DistributedLock:" + key).ConfigureAwait(false));
            }
            Assert.False(_secondLevelCache.KeyExists("DistributedLock:" + key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AcquiringAndReleasingALockShouldCreateTheKeyInRedisSync()
        {
            var key = RandomKey();
            using (_cache.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)))
            {
                Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key));
            }
            Assert.False(_secondLevelCache.KeyExists("DistributedLock:" + key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetOrAddOneKeyShouldLockTheKeyWhenTheItemIsMissingAsync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var obj = await _cache.GetOrAddAsync(key,
                                                 () =>
                                                 {
                                                     delegateWasCalled = true;

                                                     Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key));

                                                     return Task.FromResult(_testObject);
                                                 }).ConfigureAwait(false);

            Assert.True(delegateWasCalled);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetOrAddOneKeyShouldLockTheKeyWhenTheItemIsMissingSync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var obj = _cache.GetOrAdd(key,
                                      () =>
                                      {
                                          delegateWasCalled = true;

                                          Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key));

                                          return Task.FromResult(_testObject);
                                      });

            Assert.True(delegateWasCalled);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetOrAddMultipleKeyShouldLockTheKeyWhenTheItemIsMissingAsync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = await _cache.GetOrAddAsync<CachingTestClass>(keys,
                                                                      definitions =>
                                                                      {
                                                                          delegateWasCalled = true;

                                                                          Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                                          Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                                          Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key1));

                                                                          var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                                          results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                                          return Task.FromResult(results);
                                                                      })
                                     .ConfigureAwait(false);
            Assert.True(delegateWasCalled);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetOrAddMultipleKeyShouldLockTheKeyWhenTheItemIsMissingSync()
        {
            bool delegateWasCalled = false;
            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = _cache.GetOrAdd<CachingTestClass>(keys,
                                                           definitions =>
                                                           {
                                                               delegateWasCalled = true;

                                                               Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                               Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                               Assert.True(_secondLevelCache.KeyExists("DistributedLock:" + key1));

                                                               var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                               results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                               return results;
                                                           });
            Assert.True(delegateWasCalled);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task StoringANonSerializableItemIn2CachesShouldNotTriggerARemoveUnlessItemIsAlreadyInNonSerializableCacheAsync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemoved = true;
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemovedFromOtherCache = true;

            await _cache.SetAsync(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            await otherCache.SetAsync(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StoringANonSerializableItemIn2CachesShouldNotTriggerARemoveUnlessItemIsAlreadyInNonSerializableCacheSync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemoved = true;
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) => itemsWereRemovedFromOtherCache = true;

            _cache.Set(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            otherCache.Set(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task StoringMultipleNonSerializableItemIn2CachesShouldNotTriggerARemoveUnlessItemIsAlreadyInNonSerializableCacheAsync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key1 = RandomKey();
            var key2 = RandomKey();

            var objects = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            objects.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(60) }, _nonSerializableTestObject);
            objects.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);


            bool itemsWereRemoved = false;
            int itemsWereRemovedCount = 0;
            bool itemsWereRemovedFromOtherCache = false;
            int itemsWereRemovedCountFromOtherCache = 0;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };

            await _cache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);
            Assert.Equal(0, itemsWereRemovedCount);
            Assert.Equal(2, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            await otherCache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void StoringMultipleNonSerializableItemIn2CachesShouldNotTriggerARemoveUnlessItemIsAlreadyInNonSerializableCacheSync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key1 = RandomKey();
            var key2 = RandomKey();

            var objects = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            objects.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(60) }, _nonSerializableTestObject);
            objects.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);


            bool itemsWereRemoved = false;
            int itemsWereRemovedCount = 0;
            bool itemsWereRemovedFromOtherCache = false;
            int itemsWereRemovedCountFromOtherCache = 0;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };

            _cache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.True(itemsWereRemovedFromOtherCache);
            Assert.Equal(0, itemsWereRemovedCount);
            Assert.Equal(2, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            otherCache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RemovingAllKeysShouldExpireAllKeysAsync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());

            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();
            var key4 = RandomKey();

            await _cache.SetAsync(key1, _testObject).ConfigureAwait(false);
            await _cache.SetAsync(key2, _testObject).ConfigureAwait(false);
            await _cache.SetAsync(key3, _testObject).ConfigureAwait(false);
            await _cache.SetAsync(key4, _testObject).ConfigureAwait(false);

            bool removeAll = false;
            bool removeAllFromOtherCache = false;
            bool itemsWereRemoved = false;
            int itemsWereRemovedCount = 0;
            bool itemsWereRemovedFromOtherCache = false;
            int itemsWereRemovedCountFromOtherCache = 0;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };
            _secondLevelCache.OnRemoveAllItems += (sender, args) => removeAll = true;
            otherSecondLevelCache.OnRemoveAllItems += (sender, args) => removeAllFromOtherCache = true;

            await RemoveAllAndWaitAsync().ConfigureAwait(false);
            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
            Assert.True(removeAll);
            Assert.True(removeAllFromOtherCache);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            Assert.Equal(0, otherFirstLevelCache.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemovingAllKeysShouldExpireAllKeysSync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());

            var key1 = RandomKey();
            var key2 = RandomKey();
            var key3 = RandomKey();
            var key4 = RandomKey();

            _cache.Set(key1, _testObject);
            _cache.Set(key2, _testObject);
            _cache.Set(key3, _testObject);
            _cache.Set(key4, _testObject);

            bool removeAll = false;
            bool removeAllFromOtherCache = false;
            bool itemsWereRemoved = false;
            int itemsWereRemovedCount = 0;
            bool itemsWereRemovedFromOtherCache = false;
            int itemsWereRemovedCountFromOtherCache = 0;

            _secondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += (sender, e) =>
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };
            _secondLevelCache.OnRemoveAllItems += (sender, args) => removeAll = true;
            otherSecondLevelCache.OnRemoveAllItems += (sender, args) => removeAllFromOtherCache = true;

            RemoveAllAndWait();
            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.False(itemsWereRemoved);
            Assert.False(itemsWereRemovedFromOtherCache);
            Assert.True(removeAll);
            Assert.True(removeAllFromOtherCache);
            Assert.Equal(0, _firstLevelCache.NumberOfObjects);
            Assert.Equal(0, otherFirstLevelCache.NumberOfObjects);
        }



        /*
                     storing a non serializable object should store a marker in 1st and 2nd level cache

                    removing key in redis should remove key in nonserializablecache

                    getting an object placeholder from redis should not store it in first level cache and should return null

                    do for multiple get/set / async/sync

                get or add

                nonserialized cache ttl

                    // are events serialized?
                     */

    }
}
