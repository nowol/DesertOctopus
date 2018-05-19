using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Tests.Models;
using Xunit;

namespace DesertOctopus.MammothCache.Tests
{
    public class SquirrelCacheTest : BaseTest, IDisposable
    {
        private SquirrelCache _cacheRepository;
        private CachingTestClass _testObject;
        private byte[] _serializedTestObject;
        private readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private readonly IFirstLevelCacheCloningProvider _noCloningProvider = new NoCloningProvider();
        private readonly IFirstLevelCacheCloningProvider _alwaysCloningProvider = new AlwaysCloningProvider();
        private readonly IMammothCacheSerializationProvider _serializationProvider = new MammothCacheSerializationProvider();

        public SquirrelCacheTest()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = TimeSpan.FromSeconds(1);

            _cacheRepository = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);
            _testObject = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);
        }

        public void Dispose()
        {
            _cacheRepository.Dispose();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreatingASquirrelCacheWithInvalidTimerValueShoudThrowException()
        {
            var config = new FirstLevelCacheConfig();
            config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            config.MaximumMemorySize = 1000;
            config.TimerInterval = TimeSpan.FromSeconds(0);
            Assert.Throws<ArgumentException>(() => new SquirrelCache(config, _noCloningProvider, _serializationProvider));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreatingASquirrelCacheWithInvalidAbsoluteExpirationValueShoudThrowException()
        {
            var config = new FirstLevelCacheConfig();
            config.AbsoluteExpiration = TimeSpan.FromSeconds(0);
            config.MaximumMemorySize = 1000;
            config.TimerInterval = TimeSpan.FromSeconds(1);
            Assert.Throws<ArgumentException>(() => new SquirrelCache(config, _noCloningProvider, _serializationProvider));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CreatingASquirrelCacheWithInvalidMemorySizeValueShoudThrowException()
        {
            var config = new FirstLevelCacheConfig();
            config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            config.MaximumMemorySize = 0;
            config.TimerInterval = TimeSpan.FromSeconds(1);
            Assert.Throws<ArgumentException>(() => new SquirrelCache(config, _noCloningProvider, _serializationProvider));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UsingACustomSerializationProviderShouldWork()
        {
            var key = Guid.NewGuid().ToString();
            var sp = new BinaryFormatterSerializationProvider();
            var objBytes = sp.Serialize(_testObject);

            var cacheRepository = new SquirrelCache(_config, _noCloningProvider, sp);
            cacheRepository.Set(key, objBytes);
            Assert.Equal(_testObject.Value, cacheRepository.Get<CachingTestClass>(key).Value.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void AddingItemToCacheWithoutTtlShouldStoreIt()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.Equal(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ItemsShouldRespectTheAbsoluteExpiration()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.Equal(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);

            Thread.Sleep(Convert.ToInt32(_config.AbsoluteExpiration.TotalMilliseconds + 2000));

            Assert.False(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void EstimatedMemoryUsageShouldGrowWhenAddingItems()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            Assert.Equal(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            _cacheRepository.Set(key2, _serializedTestObject);
            Assert.Equal(_serializedTestObject.Length * 2, _cacheRepository.EstimatedMemorySize);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void EstimatedMemoryUsageShouldDecreaseWhenRemovingItems()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            _cacheRepository.Set(key2, _serializedTestObject);
            Assert.Equal(_serializedTestObject.Length * 2, _cacheRepository.EstimatedMemorySize);

            _cacheRepository.Remove(key1);

            WaitFor(0.5);
            Assert.Equal(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            _cacheRepository.RemoveAll();
            WaitFor(0.5);
            Assert.Equal(0, _cacheRepository.EstimatedMemorySize);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void EstimatedMemoryUsageShouldDecreaseWhenAnItemIsRemovedDueToAbsoluteExpiration()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.Equal(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);
            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.False(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
            Assert.Equal(0, _cacheRepository.EstimatedMemorySize);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemovingItemFromTheCacheShouldRemoveItFromTheStore()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.Equal(_testObject.Value, _cacheRepository.Get<CachingTestClass>(key).Value.Value);
            _cacheRepository.Remove(key);
            Assert.False(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemovingAllItemsFromTheCacheShouldRemoveThemFromTheStore()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            _cacheRepository.Set(key2, _serializedTestObject);
            _cacheRepository.RemoveAll();
            Assert.Equal(0, _cacheRepository.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectShouldBeRemovedFromTheCacheBecauseTheCacheIsOverTheConfiguredMemoryLimit()
        {
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[1000] };
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);

            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, bigSerializedTestObject);

            Assert.True(_cacheRepository.EstimatedMemorySize > _config.MaximumMemorySize);

            WaitFor(_config.TimerInterval.TotalSeconds * 2);

            Assert.Equal(0, _cacheRepository.EstimatedMemorySize);
            Assert.Equal(0, _cacheRepository.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectsShouldBeRemovedInTheOrderTheyWereAddedOnceTheCacheisOverTheConfiguredMemoryLimit()
        {
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[800] };
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);

            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            _cacheRepository.Set(key1, _serializedTestObject);
            _cacheRepository.Set(key2, bigSerializedTestObject);

            Assert.True(_cacheRepository.EstimatedMemorySize > _config.MaximumMemorySize);

            WaitFor(_config.TimerInterval.TotalSeconds * 2);

            Assert.Equal(bigSerializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            Assert.Equal(1, _cacheRepository.NumberOfObjects);
            Assert.True(_cacheRepository.Get<CachingTestClass>(key2).IsSuccessful);
            Assert.NotNull(_cacheRepository.Get<CachingTestClass>(key2).Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectsShouldBeRemovedUntilTheCacheIsNoLongerOverTheConfigureMemoryLimit()
        {
            var sw = Stopwatch.StartNew();
            var keys = new List<string>();
            int numberOfItemsToAdd = (_config.MaximumMemorySize / _serializedTestObject.Length) + 5;
            for (int i = 0; i < numberOfItemsToAdd; i++)
            {
                var key = Guid.NewGuid().ToString();
                _cacheRepository.Set(key, _serializedTestObject);
                keys.Add(key);
            }

            Assert.Equal(numberOfItemsToAdd, _cacheRepository.NumberOfObjects);
            Assert.True(_cacheRepository.EstimatedMemorySize > _config.MaximumMemorySize, _cacheRepository.EstimatedMemorySize + " > " + _config.MaximumMemorySize + " " + sw.ElapsedMilliseconds + " " + _cacheRepository.NumberOfObjects);

            WaitFor(_config.TimerInterval.TotalSeconds * 2);

            Assert.Equal(numberOfItemsToAdd - 5, _cacheRepository.NumberOfObjects);
            Assert.Equal(_serializedTestObject.Length * (numberOfItemsToAdd - 5), _cacheRepository.EstimatedMemorySize);

            foreach (var key in keys.Skip(5))
            {
                Assert.True(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);
                Assert.NotNull(_cacheRepository.Get<CachingTestClass>(key).Value);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectShouldBeReplaced()
        {
            var bigTestObject = new CachingTestClass() { ByteArray = new bool[800] };
            var bigSerializedTestObject = KrakenSerializer.Serialize(bigTestObject);

            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            WaitFor(0.5);
            Assert.Equal(_serializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            _cacheRepository.Set(key, bigSerializedTestObject);

            WaitFor(0.5);
            Assert.Equal(bigSerializedTestObject.Length, _cacheRepository.EstimatedMemorySize);
            Assert.Equal(1, _cacheRepository.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldNotNeverBeCloned()
        {
            var cache = new SquirrelCache(_config, _noCloningProvider, _serializationProvider);

            var key = RandomKey();
            cache.Set(key, _serializedTestObject);
            var obj1 = cache.Get<object>(key).Value;
            var obj2 = cache.Get<object>(key).Value;
            Assert.NotNull(obj1);
            Assert.True(ReferenceEquals(obj1, obj2));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldAlwaysBeCloned()
        {
            var cache = new SquirrelCache(_config, _alwaysCloningProvider, _serializationProvider);

            var key = RandomKey();
            cache.Set(key, _serializedTestObject);
            var obj1 = cache.Get<object>(key).Value;
            var obj2 = cache.Get<object>(key).Value;
            Assert.NotNull(obj1);
            Assert.False(ReferenceEquals(obj1, obj2));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldBeClonedIfTheyAreFromASpecificNameSpace()
        {
            var namespaceCloningProvider = new NamespacesBasedCloningProvider(new [] { "DesertOctopus.MammothCache.Tests" });
            var cache = new SquirrelCache(_config, namespaceCloningProvider, _serializationProvider);

            var testObject2 = EqualityComparer<string>.Default;
            var serializedTestObject2 = KrakenSerializer.Serialize(testObject2);

            var key1 = RandomKey();
            var key2 = RandomKey();
            cache.Set(key1, _serializedTestObject);
            cache.Set(key2, serializedTestObject2);

            var obj1_1 = cache.Get<object>(key1).Value;
            var obj1_2 = cache.Get<object>(key1).Value;
            Assert.NotNull(obj1_1);
            Assert.False(ReferenceEquals(obj1_1, obj1_2));

            var obj2_1 = cache.Get<object>(key2).Value;
            var obj2_2 = cache.Get<object>(key2).Value;
            Assert.NotNull(obj2_1);
            Assert.True(ReferenceEquals(obj2_1, obj2_2));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NamespacesBasedCloningProviderShouldReturnFalseForNullObjects()
        {
            var namespaceCloningProvider = new NamespacesBasedCloningProvider(new [] { "DesertOctopus.MammothCache.Tests" });
            Assert.False(namespaceCloningProvider.RequireCloning(null));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectRetrievedFromFirstLevelCacheShouldNotBeClonedIfTheyAreFromASpecificNameSpace()
        {
            var namespaceCloningProvider = new NamespacesBasedCloningProvider(new [] { "System" });
            var cache = new SquirrelCache(_config, namespaceCloningProvider, _serializationProvider);

            var testObject2 = EqualityComparer<string>.Default;
            var serializedTestObject2 = KrakenSerializer.Serialize(testObject2);

            var key1 = RandomKey();
            var key2 = RandomKey();
            cache.Set(key1, _serializedTestObject);
            cache.Set(key2, serializedTestObject2);

            var obj1_1 = cache.Get<object>(key1).Value;
            var obj1_2 = cache.Get<object>(key1).Value;
            Assert.NotNull(obj1_1);
            Assert.True(ReferenceEquals(obj1_1, obj1_2));

            var obj2_1 = cache.Get<object>(key2).Value;
            var obj2_2 = cache.Get<object>(key2).Value;
            Assert.NotNull(obj2_1);
            Assert.False(ReferenceEquals(obj2_1, obj2_2));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectsRemoveFromSquirrelCacheAreRemovedFromTheByAgeCache()
        {
            var key = Guid.NewGuid().ToString();
            _cacheRepository.Set(key, _serializedTestObject);
            Assert.Equal(1, _cacheRepository.NumberOfObjects);

            WaitFor(_config.AbsoluteExpiration.TotalSeconds * 2);

            Assert.False(_cacheRepository.Get<CachingTestClass>(key).IsSuccessful);

            WaitFor(1);

            Assert.Equal(0, _cacheRepository.NumberOfObjects);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NoCloningProviderShouldThrowWhenCloning()
        {
            Assert.Throws<NotImplementedException>(() => _noCloningProvider.Clone(new object()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void SquirrelCacheShouldBeThreadSafe()
        {
            var config = new FirstLevelCacheConfig();
            config.AbsoluteExpiration = TimeSpan.FromSeconds(3600);
            config.MaximumMemorySize = 100000000;
            config.TimerInterval = TimeSpan.FromSeconds(5);
            var cacheRepository = new SquirrelCache(config, _noCloningProvider, _serializationProvider);

            // seed cache
            var seededItems = 100000;
            int numberOfMod3Items = Enumerable.Range(0, seededItems).Count(x => x % 3 == 0);

            Parallel.For(0,
                         seededItems,
                         i =>
                         {
                             cacheRepository.Set("cache_" + i,
                                                  _serializedTestObject);
                         });

            Assert.Equal(seededItems,
                            cacheRepository.NumberOfObjects);
            Assert.Equal(seededItems * _serializedTestObject.Length,
                            cacheRepository.EstimatedMemorySize);

            Parallel.For(0,
                         seededItems,
                         i =>
                         {
                             if (i % 3 == 0)
                             {
                                 cacheRepository.Remove("cache_" + i);
                             }
                             else
                             {
                                 cacheRepository.Set("cache_" + i,
                                                      _serializedTestObject);
                             }
                         });

            WaitFor(10);

            Assert.Equal(seededItems - numberOfMod3Items,
                            cacheRepository.NumberOfObjects);
            Assert.Equal((seededItems - numberOfMod3Items) * _serializedTestObject.Length,
                         cacheRepository.EstimatedMemorySize);
        }


        //[Fact]
        //[Trait("Category", "Unit")]
        //public void aaa()
        //{
        //    var sw = Stopwatch.StartNew();

        //    _cacheRepository.Set("cache_0",
        //                        _serializedTestObject);

        //    int i = 0;

        //    while (true)
        //    {

        //        _cacheRepository.Set("cache_" + i++,
        //                            _serializedTestObject,
        //                            TimeSpan.FromSeconds(0.1));
        //        Thread.Sleep(500);
        //    }
        //}
    }

// add test where we cache a null value (it should be cached and return a conditional true
}
