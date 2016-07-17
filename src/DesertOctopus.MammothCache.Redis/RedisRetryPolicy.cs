using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.MammothCache.Redis
{
    /// <summary>
    /// Redis retry policy
    /// </summary>
    public class RedisRetryPolicy : IRedisRetryPolicy
    {
        /// <inheritdoc/>
        public ICollection<TimeSpan> SleepDurations { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisRetryPolicy"/> class.
        /// </summary>
        /// <param name="sleepDurations">Sleep times when retrying</param>
        public RedisRetryPolicy(params TimeSpan[] sleepDurations)
        {
            SleepDurations = sleepDurations.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisRetryPolicy"/> class.
        /// </summary>
        /// <param name="sleepDurationsInMs">Sleep times when retrying</param>
        public RedisRetryPolicy(params int[] sleepDurationsInMs)
        {
            SleepDurations = sleepDurationsInMs.Select(x => TimeSpan.FromMilliseconds(x)).ToArray();
        }
    }
}