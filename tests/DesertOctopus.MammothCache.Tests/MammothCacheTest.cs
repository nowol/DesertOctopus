using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Tests.Models;
using DesertOctupos.MammothCache.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Tests
{
    [TestClass]
    public class MammothCacheTest : BaseTest
    {
        private SquirrelCache _firstLevelCache;
        private CachingTestClass _testObject;
        private byte[] _serializedTestObject;
        private readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();

        private RedisConnection _secondLevelCache;
        private string _redisConnectionString = "172.16.100.100";
        private IRedisRetryPolicy _redisRetryPolicy;

        private MammothCache _cache;

        [TestInitialize]
        public void Initialize()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = 60;

            _firstLevelCache = new SquirrelCache(_config);
            _testObject = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _secondLevelCache = new RedisConnection(_redisConnectionString, _redisRetryPolicy);

            _cache = new MammothCache(_firstLevelCache, _secondLevelCache, new MammothCacheSerializationProvider());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _firstLevelCache.Dispose();
            _secondLevelCache.RemoveAll();
            _secondLevelCache.Dispose();
        }

        [TestMethod]
        public async Task AddingAnItemShouldAddItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        public void AddingAnItemShouldAddItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        public async Task RemovingAnItemShouldRemoveItInBothLevelOfCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            await _cache.RemoveAsync(key).ConfigureAwait(false);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
        public void RemovingAnItemShouldRemoveItInBothLevelOfCacheSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            _cache.Remove(key);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            Assert.IsNull(_secondLevelCache.Get(key));
        }

        [TestMethod]
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
        public async Task ExpiredItemFromFirstLevelShouldStillExistInSecondLevelAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        public void ExpiredItemFromFirstLevelShouldStillExistInSecondLevelSync()
        {
            var key = RandomKey();
            _cache.Set(key, _testObject, TimeSpan.FromSeconds(30));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);
        }

        [TestMethod]
        public async Task ItemShouldBePutIntoFirstLevelCacheWhenFetchFromTheSecondLevelCacheAsync()
        {
            var key = RandomKey();
            await _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            Assert.IsNotNull(await _cache.GetAsync<CachingTestClass>(key).ConfigureAwait(false));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
        public void ItemShouldBePutIntoFirstLevelCacheWhenFetchFromTheSecondLevelCacheSync()
        {
            var key = RandomKey();
            _cache.SetAsync(key, _testObject, TimeSpan.FromSeconds(30));

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_firstLevelCache.Get<CachingTestClass>(key).IsSuccessful);
            var bytes = _secondLevelCache.Get(key);
            Assert.AreEqual(_testObject.Value, KrakenSerializer.Deserialize<CachingTestClass>(bytes).Value);

            Assert.IsNotNull(_cache.Get<CachingTestClass>(key));

            Assert.AreEqual(_testObject.Value, _firstLevelCache.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
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
        public async Task GetOrAddShouldUseTheItemProviderByTheDelegateIfItIsMissingFromTheCacheAsync()
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
        }

        [TestMethod]
        public void GetOrAddShouldUseTheItemProviderByTheDelegateIfItIsMissingFromTheCacheSync()
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
        }

        [TestMethod]
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
            Assert.AreEqual(_testObject.Value, value.Value);
        }

        [TestMethod]
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
            Assert.AreEqual(_testObject.Value, value.Value);
        }
    }
}
