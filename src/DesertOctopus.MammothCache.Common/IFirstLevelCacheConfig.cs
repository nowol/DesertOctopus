using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    public interface IFirstLevelCacheConfig
    {
        int TimerInterval { get; set; }
        int MaximumMemorySize { get; set; }
        TimeSpan AbsoluteExpiration { get; set; }
    }
}