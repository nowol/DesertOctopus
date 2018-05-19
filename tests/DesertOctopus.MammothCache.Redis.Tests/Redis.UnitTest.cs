using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis.Tests.Models;
using DesertOctopus.MammothCache.Tests;
using Xunit;

namespace DesertOctopus.MammothCache.Redis.Tests
{
    public class RedisUnitTest : BaseTest
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void SleepsDurationOfTheRetryPolicyShouldBeInitialized()
        {
            var policy = new RedisRetryPolicy(TimeSpan.FromSeconds(1),
                                              TimeSpan.FromSeconds(2),
                                              TimeSpan.FromSeconds(3));
            Assert.Equal(3, policy.SleepDurations.Count);
            Assert.Equal(1, policy.SleepDurations.ElementAt(0).TotalSeconds);
            Assert.Equal(2, policy.SleepDurations.ElementAt(1).TotalSeconds);
            Assert.Equal(3, policy.SleepDurations.ElementAt(2).TotalSeconds);
        }
    }
}
