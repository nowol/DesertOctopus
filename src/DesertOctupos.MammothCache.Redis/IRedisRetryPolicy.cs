using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctupos.MammothCache.Redis
{
    public interface IRedisRetryPolicy
    {
        ICollection<TimeSpan> SleepDurations { get; }
    }

    public class RedisRetryPolicy : IRedisRetryPolicy
    {
        public ICollection<TimeSpan> SleepDurations { get; }

        public RedisRetryPolicy(IEnumerable<TimeSpan> sleepDurations)
        {
            SleepDurations = sleepDurations.ToArray();
        }

        public RedisRetryPolicy(params int[] sleepDurationsInMs)
        {
            SleepDurations = sleepDurationsInMs.Select(x => TimeSpan.FromMilliseconds(x)).ToArray();
        }
    }
}