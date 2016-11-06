using System;
using System.Linq;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Provides the configuration for the <see cref="SquirrelCache"/>
    /// </summary>
    public sealed class FirstLevelCacheConfig : IFirstLevelCacheConfig
    {
        /// <inheritdoc/>
        public TimeSpan TimerInterval { get; set; }

        /// <inheritdoc/>
        public int MaximumMemorySize { get; set; }

        /// <inheritdoc/>
        public TimeSpan AbsoluteExpiration { get; set; }
    }
}