using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        [TestInitialize]
        public void Initialize()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = 1;

            _cacheRepository = new SquirrelCache(_config);
            _testObject = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cacheRepository.Dispose();
        }

        [TestMethod]
        public void AddingItemToCacheWithoutTtlShouldStoreIt()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);
        }

        [TestMethod]
        public void ItemsShouldRespectTheAbsoluteExpiration()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);

            Thread.Sleep(Convert.ToInt32(_config.AbsoluteExpiration.TotalMilliseconds + 2000));

            Assert.IsFalse(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
        }

        [TestMethod]
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
        public void RemovingItemFromTheCacheShouldRemoveItFromTheStore()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.AreEqual(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);
            _cacheRepository.Remove(key);
            Assert.IsFalse(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
        }

        [TestMethod]
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

            Assert.AreEqual(969, _cacheRepository.EstimatedMemorySize);
            Assert.AreEqual(1, _cacheRepository.NumberOfObjects);
            Assert.IsTrue(_cacheRepository.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.IsNotNull(_cacheRepository.Get<CachingTestClass>(key2).Value);
        }

        [TestMethod]
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
            Assert.AreEqual(845, _cacheRepository.EstimatedMemorySize);

            foreach (var key in keys.Skip(5))
            {
                Assert.IsTrue(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
                Assert.IsNotNull(_cacheRepository.Get<CachingTestClass>(key).Value);
            }
        }

        [TestMethod]
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
    }
}
