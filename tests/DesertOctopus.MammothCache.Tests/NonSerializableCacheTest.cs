using System;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Tests.Models;
using Xunit;

namespace DesertOctopus.MammothCache.Tests
{
    public class NonSerializableCacheTest : BaseTest
    {
        private readonly CachingTestClass _testObject;
        private readonly NonSerializableCache _nonSerializableCache = new NonSerializableCache();

        public NonSerializableCacheTest()
        {
            _testObject = new CachingTestClass();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void AlwaysCloningProviderShouldAlwaysClone()
        {
            var cp = new AlwaysCloningProvider();
            Assert.True(cp.RequireCloning(_testObject.GetType()));
            var cloned = cp.Clone(_testObject);
            Assert.False(ReferenceEquals(cloned, _testObject));
            Assert.Equal(_testObject.Value, cloned.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UnableToAcquireLockExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new UnableToAcquireLockException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.Equal("abc", clonedEx.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NonSerializableCacheSettingANullValueShouldStoreTheNullValue()
        {
            var randomKey = RandomKey();
            _nonSerializableCache.Set(randomKey, null, TimeSpan.FromSeconds(30));
            Assert.Equal(1, _nonSerializableCache.NumberOfObjects);
            var value = _nonSerializableCache.Get<object>(randomKey);
            Assert.True(value.IsSuccessful);
            Assert.Null(value.Value);
        }
    }
}
