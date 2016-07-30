using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis.Tests.Models;
using DesertOctopus.MammothCache.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Redis.Tests
{
    [TestClass]
    public class RedisUnitTest : BaseTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void SleepsDurationOfTheRetryPolicyShouldBeInitialized()
        {
            var policy = new RedisRetryPolicy(TimeSpan.FromSeconds(1),
                                              TimeSpan.FromSeconds(2),
                                              TimeSpan.FromSeconds(3));
            Assert.AreEqual(3, policy.SleepDurations.Count);
            Assert.AreEqual(1, policy.SleepDurations.ElementAt(0).TotalSeconds);
            Assert.AreEqual(2, policy.SleepDurations.ElementAt(1).TotalSeconds);
            Assert.AreEqual(3, policy.SleepDurations.ElementAt(2).TotalSeconds);
        }
    }
}
