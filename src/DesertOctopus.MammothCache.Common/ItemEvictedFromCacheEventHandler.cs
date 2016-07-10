using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Event for object removed from a cache
    /// </summary>
    /// <param name="sender">Object that sent the event</param>
    /// <param name="e">Arguments of the object</param>
    public delegate void ItemEvictedFromCacheEventHandler(object sender, ItemEvictedEventArgs e);
}