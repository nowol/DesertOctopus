using System;
using System.Linq;

namespace DesertOctopus.MammothCache
{
    public sealed class FirstLevelCacheConfig
    {
        public int TimerInterval { get; set; }
        public int MaximumMemorySize { get; set; }
        public TimeSpan AbsoluteExpiration { get; set; }
    }
}