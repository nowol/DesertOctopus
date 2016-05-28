using System;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Tests
{
    [TestClass]
    public class MemoryCacheRepositoryTest
    {
        private MemoryCacheRepository _cacheRepository;

        [TestInitialize]
        public void Initialize()
        {
            _cacheRepository = new MemoryCacheRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cacheRepository.Dispose();
        }

        [TestMethod]
        public void AddingItemToCacheWithoutTtlShouldStoreItSync()
        {
            var key = Guid.NewGuid().ToString();
            var value = new CachingTestClass();
            _cacheRepository.Set(key, value);
            Assert.AreEqual(value, _cacheRepository.Get<CachingTestClass>(key));
        }

        [TestMethod]
        public async Task AddingItemToCacheWithoutTtlShouldStoreItASync()
        {
            var key = Guid.NewGuid().ToString();
            var value = new CachingTestClass();
            _cacheRepository.Set(key, value);
            Assert.AreEqual(value, await _cacheRepository.GetAsync<CachingTestClass>(key).ConfigureAwait(false));
        }

        [TestMethod]
        public void AddingItemToCacheWithTtlShouldStoreItSync()
        {
            var key = Guid.NewGuid().ToString();
            var value = new CachingTestClass();
            _cacheRepository.Set(key, value, ttl: TimeSpan.FromSeconds(30));
            Assert.AreEqual(value, _cacheRepository.Get<CachingTestClass>(key));
            TimeSpan? ttl;
            Assert.IsTrue(_cacheRepository.GetTimeToLive(key, out ttl));
            Assert.IsNotNull(ttl);
            Assert.IsTrue(ttl > TimeSpan.FromSeconds(25));
        }

        [TestMethod]
        public async Task AddingItemToCacheWithTtlShouldStoreItASync()
        {
            var key = Guid.NewGuid().ToString();
            var value = new CachingTestClass();
            _cacheRepository.Set(key, value, ttl: TimeSpan.FromSeconds(30));
            Assert.AreEqual(value, await _cacheRepository.GetAsync<CachingTestClass>(key).ConfigureAwait(false));
            TimeSpan? ttl;
            Assert.IsTrue(await _cacheRepository.GetTimeToLiveAsync(key, out ttl).ConfigureAwait(false));
            Assert.IsNotNull(ttl);
            Assert.IsTrue(ttl > TimeSpan.FromSeconds(25));
        }

        [TestMethod]
        public void RemovingItemFromTheCacheShouldRemoveItFromTheStoreSync()
        {
            var key = Guid.NewGuid().ToString();
            var value = new CachingTestClass();
            _cacheRepository.Set(key, value);
            Assert.AreEqual(value, _cacheRepository.Get<CachingTestClass>(key));
            _cacheRepository.Remove(key);
            Assert.IsNull(_cacheRepository.Get<CachingTestClass>(key));
        }

        [TestMethod]
        public async Task RemovingItemFromTheCacheShouldRemoveItFromTheStoreASync()
        {
            var key = Guid.NewGuid().ToString();
            var value = new CachingTestClass();
            _cacheRepository.Set(key, value);
            Assert.AreEqual(value, await _cacheRepository.GetAsync<CachingTestClass>(key).ConfigureAwait(false));
            await _cacheRepository.RemoveAsync(key).ConfigureAwait(false);
            Assert.IsNull(await _cacheRepository.GetAsync<CachingTestClass>(key).ConfigureAwait(false));
        }
    }
}
