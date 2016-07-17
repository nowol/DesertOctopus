using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Tests
{
    [TestClass]
    public class SquirrelCacheTest : BaseTest
    {
        private SquirrelCache _cacheRepository;
        private CachingTestClass _testObject;
        private byte[] _serializedTestObject;
        private readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private readonly IFirstLevelCacheCloningProvider _noCloningProvider = new NoCloningProvider();
        private readonly IFirstLevelCacheCloningProvider _alwaysCloningProvider = new AlwaysCloningProvider();

        [TestInitialize]
        public void Initialize()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = 1;

            _cacheRepository = new SquirrelCache(_config, _noCloningProvider);
            _testObject = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cacheRepository.Dispose();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void AddingItemToCacheWithoutTtlShouldStoreIt()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ItemsShouldRespectTheAbsoluteExpiration()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);

            Thread.Sleep(Convert.ToInt32(_config.AbsoluteExpiration.TotalMilliseconds + 2000));

            Assert.IsFalse(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void EstimatedMemoryUsageShouldGrowWhenAddingItems()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            Assert.AreEqual(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            _cacheRepository.Set(key2, _serializedTestObject);
            Assert.AreEqual(_serializedTestObject.Length * 2, _cacheRepository.EstimatedMemorySize);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void EstimatedMemoryUsageShouldDecreaseWhenRemovingItems()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            _cacheRepository.Set(key2, _serializedTestObject);
            Assert.AreEqual(_serializedTestObject.Length * 2, _cacheRepository.EstimatedMemorySize);

            _cacheRepository.Remove(key1);
            Assert.AreEqual(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            _cacheRepository.RemoveAll();
            Assert.AreEqual(0, _cacheRepository.EstimatedMemorySize);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void EstimatedMemoryUsageShouldDecreaseWhenAnItemIsRemovedDueToAbsoluteExpiration()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
            Assert.AreEqual(0, _cacheRepository.EstimatedMemorySize);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RemovingItemFromTheCacheShouldRemoveItFromTheStore()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);
            _cacheRepository.Remove(key);
            Assert.IsFalse(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RemovingAllItemsFromTheCacheShouldRemoveThemFromTheStore()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            _cacheRepository.Set(key2, _serializedTestObject);
            _cacheRepository.RemoveAll();
            Assert.AreEqual(0, _cacheRepository.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectShouldBeRemovedFromTheCacheBecauseTheCacheIsOverTheConfiguredMemoryLimit()
        {
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[1000]};
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);

            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, bigSerializedTestObject);

            Assert.IsTrue(_cacheRepository.EstimatedMemorySize > _config.MaximumMemorySize);

            WaitFor(_config.TimerInterval * 2);

            Assert.AreEqual(0, _cacheRepository.EstimatedMemorySize);
            Assert.AreEqual(0, _cacheRepository.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectsShouldBeRemovedInTheOrderTheyWereAddedOnceTheCacheisOverTheConfiguredMemoryLimit()
        {
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[800]};
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);

            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            _cacheRepository.Set(key2, bigSerializedTestObject);

            Assert.IsTrue(_cacheRepository.EstimatedMemorySize > _config.MaximumMemorySize);

            WaitFor(_config.TimerInterval * 2);

            Assert.AreEqual(968, _cacheRepository.EstimatedMemorySize);
            Assert.AreEqual(1, _cacheRepository.NumberOfObjects);
            Assert.IsTrue(_cacheRepository.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsNotNull(_cacheRepository.Get<CachingTestClass>(key2).Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectsShouldBeRemovedUntilTheCacheIsNoLongerOverTheConfigureMemoryLimit()
        {
            var keys = new List<string>();
            int numberOfItemsToAdd = (_config.MaximumMemorySize / _serializedTestObject.Length) + 5;
            for(int i = 0 ; i < numberOfItemsToAdd ; i++)
            {
                var key = Guid.NewGuid().ToString();
                _cacheRepository.Set(key, _serializedTestObject);
                keys.Add(key);
            }

            Assert.AreEqual(numberOfItemsToAdd, _cacheRepository.NumberOfObjects);
            Assert.IsTrue(_cacheRepository.EstimatedMemorySize > _config.MaximumMemorySize);

            WaitFor(_config.TimerInterval * 2);

            Assert.AreEqual(numberOfItemsToAdd - 5, _cacheRepository.NumberOfObjects);
            Assert.AreEqual(840, _cacheRepository.EstimatedMemorySize);

            foreach (var key in keys.Skip(5))
            {
                Assert.IsTrue(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
                Assert.IsNotNull(_cacheRepository.Get<CachingTestClass>(key).Value);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectShouldBeReplaced()
        {
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[800] };
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);

            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            _cacheRepository.Set(key, bigSerializedTestObject);

            Assert.AreEqual(bigSerializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            Assert.AreEqual(1, _cacheRepository.NumberOfObjects);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldNotNeverBeCloned()
        {
            var cache = new SquirrelCache(_config, _noCloningProvider);

            var key = RandomKey();
            cache.Set(key, _serializedTestObject);
            var obj1 = cache.Get<object>(key).Value;
            var obj2 = cache.Get<object>(key).Value;
            Assert.IsNotNull(obj1);
            Assert.IsTrue(ReferenceEquals(obj1, obj2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldAlwaysBeCloned()
        {
            var cache = new SquirrelCache(_config, _alwaysCloningProvider);

            var key = RandomKey();
            cache.Set(key, _serializedTestObject);
            var obj1 = cache.Get<object>(key).Value;
            var obj2 = cache.Get<object>(key).Value;
            Assert.IsNotNull(obj1);
            Assert.IsFalse(ReferenceEquals(obj1, obj2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldBeClonedIfTheyAreFromASpecificNameSpace()
        {
            var namespaceCloningProvider = new NamespacesBasedCloningProvider(new [] { "DesertOctopus.MammothCache.Tests" });
            var cache = new SquirrelCache(_config, namespaceCloningProvider);

            var testObject2 = EqualityComparer<string>.Default;
            var serializedTestObject2 = KrakenSerializer.Serialize(testObject2);

            var key1 = RandomKey();
            var key2 = RandomKey();
            cache.Set(key1, _serializedTestObject);
            cache.Set(key2, serializedTestObject2);

            var obj1_1 = cache.Get<object>(key1).Value;
            var obj1_2 = cache.Get<object>(key1).Value;
            Assert.IsNotNull(obj1_1);
            Assert.IsFalse(ReferenceEquals(obj1_1, obj1_2));

            var obj2_1 = cache.Get<object>(key2).Value;
            var obj2_2 = cache.Get<object>(key2).Value;
            Assert.IsNotNull(obj2_1);
            Assert.IsTrue(ReferenceEquals(obj2_1, obj2_2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void NamespacesBasedCloningProviderShouldReturnFalseForNullObjects()
        {
            var namespaceCloningProvider = new NamespacesBasedCloningProvider(new [] { "DesertOctopus.MammothCache.Tests" });
            Assert.IsFalse(namespaceCloningProvider.RequireCloning(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldNotBeClonedIfTheyAreFromASpecificNameSpace()
        {
            var namespaceCloningProvider = new NamespacesBasedCloningProvider(new [] { "System" });
            var cache = new SquirrelCache(_config, namespaceCloningProvider);

            var testObject2 = EqualityComparer<string>.Default;
            var serializedTestObject2 = KrakenSerializer.Serialize(testObject2);

            var key1 = RandomKey();
            var key2 = RandomKey();
            cache.Set(key1, _serializedTestObject);
            cache.Set(key2, serializedTestObject2);

            var obj1_1 = cache.Get<object>(key1).Value;
            var obj1_2 = cache.Get<object>(key1).Value;
            Assert.IsNotNull(obj1_1);
            Assert.IsTrue(ReferenceEquals(obj1_1, obj1_2));

            var obj2_1 = cache.Get<object>(key2).Value;
            var obj2_2 = cache.Get<object>(key2).Value;
            Assert.IsNotNull(obj2_1);
            Assert.IsFalse(ReferenceEquals(obj2_1, obj2_2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectsRemoveFromSquirrelCacheAreRemovedFromTheByAgeCache()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(1, _cacheRepository.CachedObjectsByAge.Count);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.IsFalse(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);

            WaitFor(1);

            Assert.AreEqual(0, _cacheRepository.CachedObjectsByAge.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotImplementedException))]
        public void NoCloningProviderShouldThrowWhenCloning()
        {
            _noCloningProvider.Clone(new object());
        }
    }
}
