using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis;
using DesertOctopus.MammothCache.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Tests
{
    [TestClass]
    public class MammothCacheTest : BaseTest
    {
        private SquirrelCache _firstLevelCache;
        private CachingTestClass _testObject;
        private CachingTestClass _testObject2;
        private CachingTestClass _testObject3;
        private byte[] _serializedTestObject;
        private readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private readonly IFirstLevelCacheCloningProvider _noCloningProvider = new NoCloningProvider();
        private readonly INonSerializableCache _nonSerializableCache = new NonSerializableCache();
        private readonly NotSerializableTestClass _nonSerializableTestObject = new NotSerializableTestClass();
        private readonly NotSerializableTestClass _nonSerializableTestObject2 = new NotSerializableTestClass();
        private readonly NotSerializableTestClass _nonSerializableTestObject3 = new NotSerializableTestClass();

        private RedisConnection _secondLevelCache;
        private string _redisConnectionString = "172.16.100.100";
        private IRedisRetryPolicy _redisRetryPolicy;

        private MammothCache _cache;
        private MammothCacheSerializationProvider _mammothCacheSerializationProvider;

        [TestInitialize]
        public void Initialize()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(20);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = 60;

            _firstLevelCache = new SquirrelCache(_config, _noCloningProvider);
            _testObject = new CachingTestClass();
            _testObject2 = new CachingTestClass();
            _testObject3 = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _secondLevelCache = new RedisConnection(_redisConnectionString, _redisRetryPolicy);

            _mammothCacheSerializationProvider = new MammothCacheSerializationProvider();
            _cache = new MammothCache(_firstLevelCache, _secondLevelCache, _nonSerializableCache, _mammothCacheSerializationProvider);
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                _firstLevelCache.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }

            _secondLevelCache.RemoveAll();
            _secondLevelCache.Dispose();

            try
            {
                _cache.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
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
            _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(60));

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
            await _cache.RemoveAllAsync().ConfigureAwait(false);

            var config = await _secondLevelCache.GetConfigAsync(pattern: "maxmemory").ConfigureAwait(false);
            if (config == null
                || config.Length != 1)
            {
                throw new NotSupportedException("Could not find config maxmemory");
            }
            int memoryLimit;
            if (!int.TryParse(config[0].Value, out memoryLimit)
                && memoryLimit > 0)
            {
                throw new NotSupportedException("Could not parse maxmemory: " + config[0].Value);
            }

            int slicedInto = 1000;
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[memoryLimit / slicedInto] };
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);
            var nbToStore = memoryLimit / bigSerializedTestObject.Length + 10;
            var keys = new List<string>();
            bool itemsWereRemoved = true;
            var removedKeys = new ConcurrentBag<string>();

            _secondLevelCache.OnItemRemovedFromCache += delegate(string key)
                                                        {
                                                            Assert.IsTrue(keys.Contains(key));
                                                            itemsWereRemoved = true;
                                                            removedKeys.Add(key);
                                                        };


            for (int i = 0; i < nbToStore; i++)
            {
                var key = RandomKey();
                keys.Add(key);
                await _cache.SetAsync(key, bigSerializedTestObject, TimeSpan.FromSeconds(300)).ConfigureAwait(false);
            }

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

            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key1).Value.TotalSeconds > 25);
            Assert.IsNull(_secondLevelCache.GetTimeToLive(key2));
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

            Assert.IsTrue(_secondLevelCache.GetTimeToLive(key1).Value.TotalSeconds > 25);
            Assert.IsNull(_secondLevelCache.GetTimeToLive(key2));
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

            WaitFor(10);
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

            WaitFor(10);

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
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider);
            var otherSecondLevelCache = new RedisConnection(_redisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));
            _cache.Get<CachingTestClass>(key);
            otherCache.Get<CachingTestClass>(key);

            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(1, otherFirstLevelCache.NumberOfObjects);

            WaitFor(10);

            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(0, otherFirstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void UpdatingAnItemShouldRemoveItFromAllFirstLevelCaches()
        {
            var otherFirstLevelCache = new SquirrelCache(_config, _noCloningProvider);
            var otherSecondLevelCache = new RedisConnection(_redisConnectionString, _redisRetryPolicy);
            var otherCache = new MammothCache(otherFirstLevelCache, otherSecondLevelCache, _nonSerializableCache, new MammothCacheSerializationProvider());
            var key = RandomKey();

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));
            _cache.Get<CachingTestClass>(key);
            otherCache.Get<CachingTestClass>(key);

            Assert.AreEqual(1, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(1, otherFirstLevelCache.NumberOfObjects);

            _cache.Set(key, _testObject, ttl: TimeSpan.FromSeconds(5));

            WaitFor(10);

            Assert.AreEqual(0, _firstLevelCache.NumberOfObjects);
            Assert.AreEqual(0, otherFirstLevelCache.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RemoveAllAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            await _cache.RemoveAllAsync().ConfigureAwait(false);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RemoveAll()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            _cache.RemoveAll();

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DisposingTheCacheTwiceShouldThrowAnException()
        {
            _cache.Dispose();
            _cache.Dispose();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DisposingSquirrelCacheTwiceShouldThrowAnException()
        {
            _firstLevelCache.Dispose();
            _firstLevelCache.Dispose();
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

            WaitFor(10);

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
            keys.Add(new CacheItemDefinition { Key = key1, TimeToLive = TimeSpan.FromSeconds(30) });
            keys.Add(new CacheItemDefinition { Key = key2, TimeToLive = TimeSpan.FromSeconds(10) });
            keys.Add(new CacheItemDefinition { Key = key3 });

            var values = await _cache.GetOrAddAsync<NotSerializableTestClass>(keys,
                                                                              definitions =>
                                                                              {
                                                                                  delegateWasCalled = true;
                                                                                  var results = new Dictionary<CacheItemDefinition, NotSerializableTestClass>();
                                                                                  results.Add(definitions.Single(x => x.Key == key1), _nonSerializableTestObject);

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
