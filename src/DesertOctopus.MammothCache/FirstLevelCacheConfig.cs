using System;
using System.Linq;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    public sealed class FirstLevelCacheConfig : IFirstLevelCacheConfig
    {
        public int TimerInterval { get; set; }
        public int MaximumMemorySize { get; set; }
        public TimeSpan AbsoluteExpiration { get; set; }
    }
}