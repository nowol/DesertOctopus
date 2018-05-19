using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Tests.Models;
using Xunit;

namespace DesertOctopus.MammothCache.Tests
{
    public class NullFirstLevelCacheTest : BaseTest, IDisposable
    {
        private NullFirstLevelCache _cache;

        public NullFirstLevelCacheTest()
        {
            _cache = new NullFirstLevelCache();
        }

        public void Dispose()
        {
            _cache.Dispose();
        }

        [Fact]
        public void NullFirstLevelCacheShouldNotStoreAnything()
        {
            _cache.Set("key", new byte[1]);
            Assert.Equal(0, _cache.EstimatedMemorySize);
            Assert.Equal(0, _cache.NumberOfObjects);
            Assert.False(_cache.Get<byte[]>("key").IsSuccessful);
        }

        [Fact]
        public void NullFirstLevelCacheShouldNotStoreAnythingWithTtl()
        {
            _cache.Set("key", new byte[1], TimeSpan.FromMinutes(1));
            Assert.Equal(0, _cache.EstimatedMemorySize);
            Assert.Equal(0, _cache.NumberOfObjects);
            Assert.False(_cache.Get<byte[]>("key").IsSuccessful);
        }

        [Fact]
        public void NullFirstLevelCacheShouldNotRemoveAnything()
        {
            _cache.Remove("key");
            Assert.Equal(0, _cache.EstimatedMemorySize);
            Assert.Equal(0, _cache.NumberOfObjects);
        }

        [Fact]
        public void NullFirstLevelCacheShouldNotRemoveEverything()
        {
            _cache.RemoveAll();
            Assert.Equal(0, _cache.EstimatedMemorySize);
            Assert.Equal(0, _cache.NumberOfObjects);
        }
    }
}
