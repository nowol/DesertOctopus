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
    public class NullFirstLevelCacheTest : BaseTest
    {
        private NullFirstLevelCache _cache;

        [TestInitialize]
        public void Initialize()
        {
            _cache = new NullFirstLevelCache();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.Dispose();
        }

        [TestMethod]
        public void NullFirstLevelCacheShouldNotStoreAnything()
        {
            _cache.Set("key", new byte[1]);
            Assert.AreEqual(0, _cache.EstimatedMemorySize);
            Assert.AreEqual(0, _cache.NumberOfObjects);
            Assert.IsFalse(_cache.Get<byte[]>("key").IsSuccessful);
        }

        [TestMethod]
        public void NullFirstLevelCacheShouldNotStoreAnythingWithTtl()
        {
            _cache.Set("key", new byte[1], TimeSpan.FromMinutes(1));
            Assert.AreEqual(0, _cache.EstimatedMemorySize);
            Assert.AreEqual(0, _cache.NumberOfObjects);
            Assert.IsFalse(_cache.Get<byte[]>("key").IsSuccessful);
        }

        [TestMethod]
        public void NullFirstLevelCacheShouldNotRemoveAnything()
        {
            _cache.Remove("key");
            Assert.AreEqual(0, _cache.EstimatedMemorySize);
            Assert.AreEqual(0, _cache.NumberOfObjects);
        }

        [TestMethod]
        public void NullFirstLevelCacheShouldNotRemoveEverything()
        {
            _cache.RemoveAll();
            Assert.AreEqual(0, _cache.EstimatedMemorySize);
            Assert.AreEqual(0, _cache.NumberOfObjects);
        }
    }
}
