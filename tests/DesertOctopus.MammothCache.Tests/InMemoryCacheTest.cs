using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DesertOctopus.MammothCache.Providers;
using DesertOctopus.MammothCache.Tests.Models;
using Moq;
using Xunit;

namespace DesertOctopus.MammothCache.Tests
{
    public class InMemoryCacheTest : BaseTest, IDisposable
    {
        private static TimeSpan _cleanupInterval = TimeSpan.FromSeconds(1);
        private InMemoryCache _cache = new InMemoryCache(_cleanupInterval, 1000);
        private readonly CachingTestClass _testObject1 = new CachingTestClass();
        private readonly CachingTestClass _testObject2 = new CachingTestClass();


        public void Dispose()
        {
            _cache.Dispose();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CallingDisposeTwiceShouldNotCrash()
        {
            _cache.Dispose();
            _cache.Dispose();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CountShouldReturnTheNumberOfItemsInTheCache()
        {
            Assert.Equal(0, _cache.Count);
            _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 1, Value = _testObject1 });
            Assert.Equal(1, _cache.Count);
            Assert.Equal(1, _cache.OrderedCacheKeysCount);
            Assert.Equal(1, _cache.EstimatedMemorySize);
            _cache.Add(_testObject2.Value.ToString(), new CachedObject { ObjectSize = 2, Value = _testObject2 });
            Assert.Equal(2, _cache.Count);
            Assert.Equal(2, _cache.OrderedCacheKeysCount);
            Assert.Equal(3, _cache.EstimatedMemorySize);
            _cache.Remove(_testObject1.Value.ToString());
            Assert.Equal(1, _cache.Count);
            Assert.Equal(1, _cache.OrderedCacheKeysCount);
            Assert.Equal(2, _cache.EstimatedMemorySize);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemoveShouldRemoveTheItemFromTheCache()
        {
            Assert.Equal(0, _cache.Count);
            _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 1, Value = _testObject1 });
            Assert.Equal(1, _cache.Count);
            Assert.Equal(1, _cache.OrderedCacheKeysCount);
            Assert.Equal(1, _cache.EstimatedMemorySize);
            Assert.NotNull(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));

            _cache.Remove(_testObject1.Value.ToString());

            Assert.Equal(0, _cache.Count);
            Assert.Equal(0, _cache.OrderedCacheKeysCount);
            Assert.Equal(0, _cache.EstimatedMemorySize);
            Assert.Null(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemoveAllShouldRemoveEveryItems()
        {
            Assert.Equal(0, _cache.Count);
            _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 1, Value = _testObject1 });
            _cache.Add(_testObject2.Value.ToString(), new CachedObject { ObjectSize = 2, Value = _testObject2 });
            Assert.Equal(2, _cache.Count);
            Assert.Equal(2, _cache.OrderedCacheKeysCount);
            Assert.Equal(3, _cache.EstimatedMemorySize);
            Assert.NotNull(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));
            Assert.NotNull(_cache.OrderedCacheKeysGet(_testObject2.Value.ToString()));
            _cache.RemoveAll();
            Assert.Equal(0, _cache.Count);
            Assert.Equal(0, _cache.OrderedCacheKeysCount);
            Assert.Equal(0, _cache.EstimatedMemorySize);
            Assert.Null(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));
            Assert.Null(_cache.OrderedCacheKeysGet(_testObject2.Value.ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemovingAnUnknownKeyShouldNotCrash()
        {
            _cache.Remove(Guid.NewGuid().ToString());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void AddingANullCacheObjectShouldNotCacheIt()
        {
            _cache.Add(Guid.NewGuid().ToString(), null);
            Assert.Equal(0, _cache.Count);
            Assert.Equal(0, _cache.OrderedCacheKeysCount);
            Assert.Equal(0, _cache.EstimatedMemorySize);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UpdateAnExistingCachedObject()
        {
            _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 1, Value = _testObject1 });
            Assert.Equal(1, _cache.Count);
            Assert.Equal(1, _cache.OrderedCacheKeysCount);
            Assert.Equal(1, _cache.EstimatedMemorySize);
            Assert.NotNull(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));

            _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 2, Value = _testObject1 });
            Assert.Equal(1, _cache.Count);
            Assert.Equal(1, _cache.OrderedCacheKeysCount);
            Assert.Equal(2, _cache.EstimatedMemorySize);
            Assert.NotNull(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void AddingWhenDisposedShouldThrowException()
        {
            _cache.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 1, Value = _testObject1 }));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemovingWhenDisposedShouldThrowException()
        {
            _cache.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _cache.Remove(_testObject1.Value.ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemovingAllWhenDisposedShouldThrowException()
        {
            _cache.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _cache.RemoveAll());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetAnUnknownObjectFromCacheShouldReturnNull()
        {
            Assert.Null(_cache.Get(Guid.NewGuid().ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetObjectFromCache()
        {
            var objectToCache = new CachedObject { ObjectSize = 1, Value = _testObject1 };
            _cache.Add(_testObject1.Value.ToString(), objectToCache);
            Assert.NotNull(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));
            var returnedObject = _cache.Get(_testObject1.Value.ToString());
            Assert.NotNull(returnedObject);
            Assert.Same(returnedObject, objectToCache);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetExpiredObjectFromCacheShouldReturnNull()
        {
            _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 1, Value = _testObject1, ExpireAt = DateTime.UtcNow.AddDays(-1) });
            Assert.Null(_cache.Get(_testObject1.Value.ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ExpiredObjectAreAutomaticallyRemovedFromCache()
        {
            _cache.Add(_testObject1.Value.ToString(), new CachedObject { ObjectSize = 1, Value = _testObject1, ExpireAt = DateTime.UtcNow.AddDays(-1) });
            Assert.Equal(1, _cache.Count);
            Assert.Equal(1, _cache.OrderedCacheKeysCount);
            Assert.Equal(1, _cache.EstimatedMemorySize);

            WaitFor(_cleanupInterval.TotalSeconds * 2);

            Assert.Equal(0, _cache.Count);
            Assert.Equal(0, _cache.OrderedCacheKeysCount);
            Assert.Equal(0, _cache.EstimatedMemorySize);
            Assert.Null(_cache.OrderedCacheKeysGet(_testObject1.Value.ToString()));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GoingOverTheAllowedMemoryShouldTriggerAnImmediateCleanUp()
        {
            var timeMock = new Mock<ICurrentDateAndTimeProvider>(MockBehavior.Strict);
            timeMock.Setup(x => x.GetCurrentDateTime()).Returns(DateTime.UtcNow.AddDays(-1));

            using (InMemoryCache cache = new InMemoryCache(_cleanupInterval, 1000, timeMock.Object))
            {
                cache.SetLastExpirationScan(DateTime.MinValue);
                cache.Add(Guid.NewGuid().ToString(), new CachedObject { ObjectSize = 10000, Value = _testObject1, ExpireAt = DateTime.UtcNow.AddDays(-1) });
                WaitFor(0.1);
                Assert.Equal(0, cache.Count);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void AddingLargeItemsToCacheShouldDelayTheCleanUpUntilMaximumAllowedItemIsReached()
        {
            var cleanupInterval = TimeSpan.FromSeconds(5);

            using (InMemoryCache cache = new InMemoryCache(cleanupInterval, 1000))
            {
                var sw = Stopwatch.StartNew();
                var cpt = 0;
                while (sw.Elapsed < cleanupInterval.Subtract(TimeSpan.FromMilliseconds(200)))
                {
                    cache.Add(Guid.NewGuid().ToString(), new CachedObject { ObjectSize = 10000, Value = _testObject1, ExpireAt = DateTime.UtcNow.AddDays(-1) });
                    WaitFor(0.1);
                    cpt++;

                    Assert.NotEqual(0, cache.Count);
                }

                WaitFor(0.5);

                Assert.Equal(0, cache.Count);
            }
        }
    }
}
