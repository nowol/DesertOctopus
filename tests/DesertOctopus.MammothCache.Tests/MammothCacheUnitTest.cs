using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis;
using DesertOctopus.MammothCache.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Tests
{
    [TestClass]
    public class MammothCacheUnitTest : BaseTest
    {
        private CachingTestClass _testObject;
        private readonly NonSerializableCache _nonSerializableCache = new NonSerializableCache();

        [TestInitialize]
        public void Initialize()
        {
            _testObject = new CachingTestClass();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void AlwaysCloningProviderShouldAlwaysClone()
        {
            var cp = new AlwaysCloningProvider();
            Assert.IsTrue(cp.RequireCloning(_testObject.GetType()));
            var cloned = cp.Clone(_testObject);
            Assert.IsFalse(ReferenceEquals(cloned, _testObject));
            Assert.AreEqual(_testObject.Value, cloned.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void UnableToAcquireLockExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new UnableToAcquireLockException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.AreEqual("abc", clonedEx.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void NonSerializableCacheSettingANullValueShouldDoNothing()
        {
            _nonSerializableCache.Set(RandomKey(), null, TimeSpan.FromSeconds(30));
            Assert.AreEqual(0, _nonSerializableCache.NumberOfObjects);
        }
    }
}
