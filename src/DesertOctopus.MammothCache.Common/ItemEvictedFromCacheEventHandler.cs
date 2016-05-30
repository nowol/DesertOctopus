using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Event for object removed from a cache
    /// </summary>
    /// <param name="key">Key of the object</param>
    public delegate void ItemEvictedFromCacheEventHandler(string key);
}