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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Tests
{
    [TestClass]
    public class MammothCacheIntegrationTest : BaseTest
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

        [TestInitialize]
        public void Initialize()
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

        [TestCleanup]
        public void Cleanup()
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

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddingAnItemShouldAddItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void AddingAnItemShouldAddItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RemovingAnItemShouldRemoveItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            await _cache.RemoveAsync(key).ConfigureAwait(false);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RemovingAnItemShouldRemoveItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            _cache.Remove(key);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task UpdatingAnItemShouldRemoveItFromOtherDistributedCacheAsync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemoved = true; };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemovedFromOtherCache = true; };

            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            await otherCache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsTrue(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void UpdatingAnItemShouldRemoveItFromOtherDistributedCacheSync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemoved = true; };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemovedFromOtherCache = true; };

            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            otherCache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsTrue(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            _secondLevelCache.OnItemRemovedFromCache += delegate(object sender, ItemEvictedEventArgs e)
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate(object sender, ItemEvictedEventArgs e)
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };

            await _cache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);
            Assert.AreEqual(0, itemsWereRemovedCount);
            Assert.AreEqual(3, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            await otherCache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsTrue(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
            Assert.AreEqual(3, itemsWereRemovedCount);
            Assert.AreEqual(0, itemsWereRemovedCountFromOtherCache);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            _secondLevelCache.OnItemRemovedFromCache += delegate(object sender, ItemEvictedEventArgs e)
                                                        {
                                                            itemsWereRemoved = true;
                                                            Interlocked.Increment(ref itemsWereRemovedCount);
                                                        };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate(object sender, ItemEvictedEventArgs e)
                                                            {
                                                                itemsWereRemovedFromOtherCache = true;
                                                                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
                                                            };

            _cache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);
            Assert.AreEqual(0, itemsWereRemovedCount);
            Assert.AreEqual(3, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            otherCache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsTrue(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
            Assert.AreEqual(3, itemsWereRemovedCount);
            Assert.AreEqual(0, itemsWereRemovedCountFromOtherCache);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task UpdatingAnItemShouldUpdateItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            var testObject2 = new CachingTestClass();
            await _cache.SetAsync(key, testObject2, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.AreEqual(testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(testObject2.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void UpdatingAnItemShouldUpdateItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            var testObject2 = new CachingTestClass();
            _cache.Set(key, testObject2, TimeSpan.FromSeconds(30));

            Assert.AreEqual(testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(testObject2.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task ExpiredItemFromFirstLevelShouldStillExistInSecondLevelAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.IsNotNull(bytes);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ExpiredItemFromFirstLevelShouldStillExistInSecondLevelSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.IsNotNull(bytes);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task ItemShouldBePutIntoFirstLevelCacheWhenFetchFromTheSecondLevelCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            Assert.IsNotNull(await _cache.GetAsync<CachingTestClass>(key).ConfigureAwait(false));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ItemShouldBePutIntoFirstLevelCacheWhenFetchFromTheSecondLevelCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            Assert.IsNotNull(_cache.Get<CachingTestClass>(key));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RemovingAnItemFromTheSecondLevelCacheShouldRemoveItFromTheFirstLevelCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            _secondLevelCache.Remove(key);
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RemovingAnItemFromTheSecondLevelCacheShouldRemoveItFromTheFirstLevelCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            _secondLevelCache.Remove(key);
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task ItemIsRemovedFromFirstLevelCacheIfItExpiresFromSecondLevelCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            _secondLevelCache.Set(key, _serializedTestObject, TimeSpan.FromMilliseconds(200));
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ItemIsRemovedFromFirstLevelCacheIfItExpiresFromSecondLevelCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            _secondLevelCache.Set(key, _serializedTestObject, TimeSpan.FromMilliseconds(200));
            WaitFor(2);
            _secondLevelCache.Get(key);
            WaitFor(2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task ItemsEvictedFromBecauseOfMemoryPressureShouldBeRemovedFromFirstLevelCacheAsync()
        {
            var c = _secondLevelCache.GetConfig(null);
            var config = await _secondLevelCache.GetConfigAsync(pattern: "maxmemory").ConfigureAwait(false);
            var memoryStr = GetAppSetting("RedisMaxMemory");
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
            var nbToStore = memoryLimit / bigSerializedTestObject.Length + 10;
            var keys = new List<string>();
            bool itemsWereRemoved = true;
            var removedKeys = new ConcurrentBag<string>();

            _secondLevelCache.OnItemRemovedFromCache += delegate(object sender, ItemEvictedEventArgs e)
                                                        {
                                                            Assert.IsTrue(keys.Contains(e.Key));
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

            Assert.IsTrue(itemsWereRemoved);

            foreach (var key in removedKeys.Take(10).ToArray())
            {
                Assert.IsFalse(_firstLevelCache.Get<object>(key).IsSuccessful);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(_testObject.Value, value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);

            var bytes = await _secondLevelCache.GetAsync(key).ConfigureAwait(false);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.AreEqual(_testObject.Value, deserializedValue.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(_testObject.Value, value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);

            var bytes = _secondLevelCache.Get(key);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.AreEqual(_testObject.Value, deserializedValue.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsFalse(delegateWasCalled);
            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(_testObject.Value, value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsFalse(delegateWasCalled);
            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(_testObject.Value, value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(_testObject.Value, values.First().Value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);

            var bytes = await _secondLevelCache.GetAsync(key1).ConfigureAwait(false);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.AreEqual(_testObject.Value, deserializedValue.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(_testObject.Value, values.First().Value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);

            var bytes = _secondLevelCache.Get(key1);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize<CachingTestClass>(bytes);
            Assert.AreEqual(_testObject.Value, deserializedValue.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            Assert.AreEqual(3, _firstLevelCache.NumberOfObjects);
            Assert.IsTrue(await _secondLevelCache.KeyExistsAsync(key1).ConfigureAwait(false));
            Assert.IsTrue(await _secondLevelCache.KeyExistsAsync(key1).ConfigureAwait(false));
            Assert.IsTrue(await _secondLevelCache.KeyExistsAsync(key1).ConfigureAwait(false));

            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key2).TimeToLive.Value.TotalSeconds > 5);
            Assert.IsFalse(_secondLevelCache.GetTimeToLive(key3).TimeToLive.HasValue);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            Assert.AreEqual(3, _firstLevelCache.NumberOfObjects);
            Assert.IsTrue(_secondLevelCache.KeyExists(key1));
            Assert.IsTrue(_secondLevelCache.KeyExists(key1));
            Assert.IsTrue(_secondLevelCache.KeyExists(key1));

            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key2).TimeToLive.Value.TotalSeconds > 5);
            Assert.IsFalse(_secondLevelCache.GetTimeToLive(key3).TimeToLive.HasValue);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsFalse(delegateWasCalled);
            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(_testObject.Value, values.First().Value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.AreEqual(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.AreEqual(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsFalse(delegateWasCalled);
            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(_testObject.Value, values.First().Value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.AreEqual(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.AreEqual(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
                                                                          Assert.AreEqual(1, definitions.Length);
                                                                          delegateWasCalled = true;
                                                                          var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                                          results.Add(definitions.Single(x => x.Key == key3), _testObject3);
                                                                          return Task.FromResult(results);
                                                                      })
                                     .ConfigureAwait(false);
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(_testObject.Value, values.First().Value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.AreEqual(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.AreEqual(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
                                                               Assert.AreEqual(1, definitions.Length);
                                                               delegateWasCalled = true;
                                                               var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                               results.Add(definitions.Single(x => x.Key == key3), _testObject3);
                                                               return results;
                                                           });
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(_testObject.Value, values.First().Value.Value);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key1).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsTrue(_firstLevelCache.Get<CachingTestClass>(key3).IsSuccessful);
            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key1).Value.Value);
            Assert.AreEqual(_testObject2.Value, _firstLevelCache.Get<CachingTestClass>(key2).Value.Value);
            Assert.AreEqual(_testObject3.Value, _firstLevelCache.Get<CachingTestClass>(key3).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(2, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(_testObject.Value, values.First(x => x.Key.Key == key1).Value.Value);
            Assert.AreEqual(testObject2.Value, values.First(x => x.Key.Key == key2).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(2, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(_testObject.Value, values.First(x => x.Key.Key == key1).Value.Value);
            Assert.AreEqual(testObject2.Value, values.First(x => x.Key.Key == key2).Value.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SetMultipleValuesAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            await _cache.SetAsync(values).ConfigureAwait(false);
            Assert.AreEqual(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();
            Assert.AreEqual(_testObject.Value, _cache.Get<CachingTestClass>(key1).Value);
            Assert.AreEqual(testObject2.Value, _cache.Get<CachingTestClass>(key2).Value);

            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.IsNull(_secondLevelCache.GetTimeToLive(key2).TimeToLive);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SetMultipleValuesSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            _cache.Set(values);

            Assert.AreEqual(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();
            Assert.AreEqual(_testObject.Value, _cache.Get<CachingTestClass>(key1).Value);
            Assert.AreEqual(testObject2.Value, _cache.Get<CachingTestClass>(key2).Value);

            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key1).TimeToLive.Value.TotalSeconds > 25);
            Assert.IsNull(_secondLevelCache.GetTimeToLive(key2).TimeToLive);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MultipleItemsRetrievedShouldHaveTheirTtlSetAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(5) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            await _cache.SetAsync(values).ConfigureAwait(false);
            Assert.AreEqual(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            values = await _cache.GetAsync<CachingTestClass>(keys).ConfigureAwait(false);
            _secondLevelCache.Set(key1, _serializedTestObject, TimeSpan.FromSeconds(30));

            WaitFor(5);
            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds);

            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void MultipleItemsRetrievedShouldHaveTheirTtlSetSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();
            var testObject2 = new CachingTestClass();

            var values = new Dictionary<CacheItemDefinition, CachingTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(5) }, _testObject);
            values.Add(new CacheItemDefinition { Key = key2 }, testObject2);

            _cache.Set(values);
            Assert.AreEqual(2, _firstLevelCache.NumberOfObjects);
            _firstLevelCache.RemoveAll();

            var keys = new List<CacheItemDefinition>();
            keys.Add(new CacheItemDefinition { Key = key1 });
            keys.Add(new CacheItemDefinition { Key = key2 });

            values = _cache.Get<CachingTestClass>(keys);
            _secondLevelCache.Set(key1, _serializedTestObject, TimeSpan.FromSeconds(30));

            WaitFor(5);

            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds);

            _firstLevelCache.Get<CachingTestClass>(key1);
            _firstLevelCache.Get<CachingTestClass>(key2);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RemovingAnItemShouldRemoveItFromAllMammothCaches()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));
            _cache.Get<CachingTestClass>(key);
            otherCache.Get<CachingTestClass>(key);

            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(1, otherFirstLevelCache.NumberOfObjects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(0, otherFirstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void UpdatingAnItemShouldRemoveItFromAllFirstLevelCaches()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));
            _cache.Get<CachingTestClass>(key);
            otherCache.Get<CachingTestClass>(key);

            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(1, otherFirstLevelCache.NumberOfObjects);

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(0, otherFirstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RemoveAllShouldEmptyTheCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            await RemoveAllAndWaitAsync().ConfigureAwait(false);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RemoveAllShouldEmptyTheCache()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            RemoveAllAndWait();

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UsingGetOrAddSingleItemWithoutDelegateShouldThrowAnExceptionAsync()
        {
            await _cache.GetOrAddAsync<CachingTestClass>(RandomKey(), null).ConfigureAwait(false);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UsingGetOrAddSingleItemWithoutDelegateShouldThrowAnExceptionSync()
        {
            _cache.GetOrAdd<CachingTestClass>(RandomKey(), null);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task ItemIsFetchedFromFirstLevelCacheAsync()
        {
            var key = RandomKey();
            _firstLevelCache.Set(key, _serializedTestObject);

            var obj = await _cache.GetAsync<CachingTestClass>(key).ConfigureAwait(false);
            Assert.AreEqual(_testObject.Value, obj.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ItemIsFetchedFromFirstLevelCacheSync()
        {
            var key = RandomKey();
            _firstLevelCache.Set(key, _serializedTestObject);

            var obj = _cache.Get<CachingTestClass>(key);
            Assert.AreEqual(_testObject.Value, obj.Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task CachingANullObjectDoesNothingAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync<CachingTestClass>(key, null).ConfigureAwait(false);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CachingANullObjectDoesNothingSync()
        {
            var key = RandomKey();
            _cache.Set<CachingTestClass>(key, null);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DeserializeObject()
        {
            var obj = _mammothCacheSerializationProvider.Deserialize(_serializedTestObject) as CachingTestClass;
            Assert.AreEqual(_testObject.Value, obj.Value);
        }





        [TestMethod]
        [TestCategory("Integration")]
        public async Task CachingANonSerializableObjectShouldStoreItInNonSerializableCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);

            Assert.AreEqual(1, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CachingANonSerializableObjectShouldStoreItInNonSerializableCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);

            Assert.AreEqual(1, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task CachingMultipleNonSerializableObjectShouldStoreItInNonSerializableCacheAsync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);
            
            await _cache.SetAsync(values).ConfigureAwait(false);

            Assert.AreEqual(2, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            foreach (var key in new [] { key1, key2 })
            {
                var bytes = _secondLevelCache.Get(key);
                Assert.IsNotNull(bytes);
                var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
                Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CachingMultipleNonSerializableObjectShouldStoreItInNonSerializableCacheSync()
        {
            var key1 = RandomKey();
            var key2 = RandomKey();

            var values = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
            values.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) }, _nonSerializableTestObject);
            values.Add(new CacheItemDefinition { Key = key2 }, _nonSerializableTestObject2);
            
            _cache.Set(values);

            Assert.AreEqual(2, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            foreach (var key in new [] { key1, key2 })
            {
                var bytes = _secondLevelCache.Get(key);
                Assert.IsNotNull(bytes);
                var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
                Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject, objs.Single(x => x.Value.Value == _nonSerializableTestObject.Value).Value));
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject2, objs.Single(x => x.Value.Value == _nonSerializableTestObject2.Value).Value));
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject, objs.Single(x => x.Value.Value == _nonSerializableTestObject.Value).Value));
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject2, objs.Single(x => x.Value.Value == _nonSerializableTestObject2.Value).Value));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RetrievinMultipleANonSerializableObjectShouldReturnTheSameReferenceAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);
            var obj = await _cache.GetAsync<NotSerializableTestClass>(key).ConfigureAwait(false);
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RetrievinMultipleANonSerializableObjectShouldReturnTheSameReferenceSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);
            var obj = _cache.Get<NotSerializableTestClass>(key);
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RetrievingANonSerializableObjectShouldReturnTheSameReferenceAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);
            var obj = await _cache.GetAsync<NotSerializableTestClass>(key).ConfigureAwait(false);
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RetrievingANonSerializableObjectShouldReturnTheSameReferenceSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);
            var obj = _cache.Get<NotSerializableTestClass>(key);
            Assert.IsTrue(ReferenceEquals(_nonSerializableTestObject, obj));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RetrievingANonSerializableObjectFromSecondLevelCacheShouldReturnNullAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _nonSerializableTestObject).ConfigureAwait(false);
            _nonSerializableCache.RemoveAll();
            Assert.IsNull(await _cache.GetAsync<NotSerializableTestClass>(key).ConfigureAwait(false));
            Assert.AreEqual(0, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RetrievingANonSerializableObjectFromSecondLevelCacheShouldReturnNullSync()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject);
            _nonSerializableCache.RemoveAll();
            Assert.IsNull(_cache.Get<NotSerializableTestClass>(key));
            Assert.AreEqual(0, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.AreEqual(0, objs.Count);
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.AreEqual(0, objs.Count);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RemovingANonSerializableObjectPlaceHolderFromSecondLevelCacheShouldRemoveItFromTheNonSerializableCache()
        {
            var key = RandomKey();
            _cache.Set(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(5));
            Assert.IsTrue(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ANonSerializableSystemTypeShouldBeStoredInTheNonSerializableCache()
        {
            using (var ms = new MemoryStream())
            {
                var obj = new System.IO.BinaryReader(ms);

                var key = RandomKey();
                _cache.Set(key, obj);
                Assert.IsNull(_cache.Get<NotSerializableTestClass>(key));
                Assert.AreEqual(1, _nonSerializableCache.NumberOfObjects);
                Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
                var bytes = _secondLevelCache.Get(key);
                Assert.IsNotNull(bytes);
                var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
                Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
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
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(_nonSerializableTestObject.Value, value.Value);
            Assert.IsTrue(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            Assert.IsFalse(_firstLevelCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ANonSerializableObjectShouldBeReturnedByGetOrAddSync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var value =  _cache.GetOrAdd<NotSerializableTestClass>(key,
                                                           () =>
                                                           {
                                                               delegateWasCalled = true;
                                                               return _nonSerializableTestObject;
                                                           },
                                                           TimeSpan.FromSeconds(30));
            Assert.IsTrue(delegateWasCalled);
            Assert.AreEqual(_nonSerializableTestObject.Value, value.Value);
            Assert.IsTrue(_nonSerializableCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            Assert.IsFalse(_firstLevelCache.Get<NotSerializableTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(1, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key1);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);

            Assert.IsNull(_secondLevelCache.Get(key2));
            Assert.IsNull(_secondLevelCache.Get(key3));
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            var values =  _cache.GetOrAdd<NotSerializableTestClass>(keys,
                                                                    definitions =>
                                                                    {
                                                                        delegateWasCalled = true;
                                                                        var results = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
                                                                        results.Add(definitions.Single(x => x.Key == key1), _nonSerializableTestObject);

                                                                        return results;
                                                                    });

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(1, _nonSerializableCache.NumberOfObjects);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            var bytes = _secondLevelCache.Get(key1);
            Assert.IsNotNull(bytes);
            var deserializedValue = _mammothCacheSerializationProvider.Deserialize(bytes);
            Assert.IsTrue(deserializedValue is NonSerializableObjectPlaceHolder);

            Assert.IsNull(_secondLevelCache.Get(key2));
            Assert.IsNull(_secondLevelCache.Get(key3));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AcquiringAndReleasingALockShouldCreateTheKeyInRedisAsync()
        {
            var key = RandomKey();
            using (await _cache.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Assert.IsTrue(await _secondLevelCache.KeyExistsAsync("DistributedLock:" + key).ConfigureAwait(false));
            }
            Assert.IsFalse(_secondLevelCache.KeyExists("DistributedLock:" + key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void AcquiringAndReleasingALockShouldCreateTheKeyInRedisSync()
        {
            var key = RandomKey();
            using (_cache.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)))
            {
                Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key));
            }
            Assert.IsFalse(_secondLevelCache.KeyExists("DistributedLock:" + key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetOrAddOneKeyShouldLockTheKeyWhenTheItemIsMissingAsync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var obj = await _cache.GetOrAddAsync(key,
                                                 () =>
                                                 {
                                                     delegateWasCalled = true;

                                                     Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key));

                                                     return Task.FromResult(_testObject);
                                                 }).ConfigureAwait(false);

            Assert.IsTrue(delegateWasCalled);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetOrAddOneKeyShouldLockTheKeyWhenTheItemIsMissingSync()
        {
            bool delegateWasCalled = false;
            var key = RandomKey();
            var obj = _cache.GetOrAdd(key,
                                      () =>
                                      {
                                          delegateWasCalled = true;

                                          Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key));

                                          return Task.FromResult(_testObject);
                                      });

            Assert.IsTrue(delegateWasCalled);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

                                                                          Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                                          Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                                          Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key1));

                                                                          var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                                          results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                                          return Task.FromResult(results);
                                                                      })
                                     .ConfigureAwait(false);
            Assert.IsTrue(delegateWasCalled);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

                                                               Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                               Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key1));
                                                               Assert.IsTrue(_secondLevelCache.KeyExists("DistributedLock:" + key1));

                                                               var results = new Dictionary<CacheItemDefinition, CachingTestClass>();
                                                               results.Add(definitions.Single(x => x.Key == key1), _testObject);
                                                               return results;
                                                           });
            Assert.IsTrue(delegateWasCalled);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task StoringANonSerializableItemIn2CachesShouldNotTriggerARemoveUnlessItemIsAlreadyInNonSerializableCacheAsync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemoved = true; };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemovedFromOtherCache = true; };

            await _cache.SetAsync(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            await otherCache.SetAsync(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void StoringANonSerializableItemIn2CachesShouldNotTriggerARemoveUnlessItemIsAlreadyInNonSerializableCacheSync()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            var otherSecondLevelCache = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            bool itemsWereRemoved = false;
            bool itemsWereRemovedFromOtherCache = false;

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemoved = true; };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e) { itemsWereRemovedFromOtherCache = true; };

            _cache.Set(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedFromOtherCache = false;

            otherCache.Set(key, _nonSerializableTestObject, ttl: TimeSpan.FromSeconds(60));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemoved = true;
                Interlocked.Increment(ref itemsWereRemovedCount);
            };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemovedFromOtherCache = true;
                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
            };

            await _cache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);
            Assert.AreEqual(0, itemsWereRemovedCount);
            Assert.AreEqual(2, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            await otherCache.SetAsync(objects).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemoved = true;
                Interlocked.Increment(ref itemsWereRemovedCount);
            };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemovedFromOtherCache = true;
                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
            };

            _cache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsTrue(itemsWereRemovedFromOtherCache);
            Assert.AreEqual(0, itemsWereRemovedCount);
            Assert.AreEqual(2, itemsWereRemovedCountFromOtherCache);

            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;

            otherCache.Set(objects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
            itemsWereRemoved = false;
            itemsWereRemovedCount = 0;
            itemsWereRemovedFromOtherCache = false;
            itemsWereRemovedCountFromOtherCache = 0;
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemoved = true;
                Interlocked.Increment(ref itemsWereRemovedCount);
            };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemovedFromOtherCache = true;
                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
            };
            _secondLevelCache.OnRemoveAllItems += delegate { removeAll = true; };
            otherSecondLevelCache.OnRemoveAllItems += delegate { removeAllFromOtherCache = true; };

            await RemoveAllAndWaitAsync().ConfigureAwait(false);
            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
            Assert.IsTrue(removeAll);
            Assert.IsTrue(removeAllFromOtherCache);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(0, otherFirstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
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

            _secondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemoved = true;
                Interlocked.Increment(ref itemsWereRemovedCount);
            };
            otherSecondLevelCache.OnItemRemovedFromCache += delegate (object sender, ItemEvictedEventArgs e)
            {
                itemsWereRemovedFromOtherCache = true;
                Interlocked.Increment(ref itemsWereRemovedCountFromOtherCache);
            };
            _secondLevelCache.OnRemoveAllItems += delegate { removeAll = true; };
            otherSecondLevelCache.OnRemoveAllItems += delegate { removeAllFromOtherCache = true; };

            RemoveAllAndWait();
            WaitFor(_config.AbsoluteExpiration.TotalSeconds + 5);

            Assert.IsFalse(itemsWereRemoved);
            Assert.IsFalse(itemsWereRemovedFromOtherCache);
            Assert.IsTrue(removeAll);
            Assert.IsTrue(removeAllFromOtherCache);
            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(0, otherFirstLevelCache.NumberOfObjects);
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
