using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctupos.MammothCache.Redis
{
    /// <summary>
    /// Represents the contract for classes that wishes to provide a retry policy for Redis
    /// </summary>
    public interface IRedisRetryPolicy
    {
        /// <summary>
        /// Gets the time to sleep when retrying
        /// </summary>
        ICollection<TimeSpan> SleepDurations { get; }
    }
}