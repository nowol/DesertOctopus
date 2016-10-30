using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Represents the contract for classes that wishes to implements the configuration of a first level cache
    /// </summary>
    public interface IFirstLevelCacheConfig
    {
        /// <summary>
        /// Gets or sets the interval of the cleanup timer
        /// </summary>
        TimeSpan TimerInterval { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory in bytes
        /// </summary>
        int MaximumMemorySize { get; set; }

        /// <summary>
        /// Gets or sets absolute expiration for objects stored in the cache
        /// </summary>
        TimeSpan AbsoluteExpiration { get; set; }
    }
}