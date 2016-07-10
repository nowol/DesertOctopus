using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Event when all items are removed
    /// </summary>
    /// <param name="sender">Object that sent the event</param>
    /// <param name="e">Arguments of the event</param>
    public delegate void RemoveAllItemsEventHandler(object sender, RemoveAllItemsEventArgs e);
}