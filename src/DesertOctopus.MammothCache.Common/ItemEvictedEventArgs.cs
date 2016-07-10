using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Arguments for item eviction
    /// </summary>
    public class ItemEvictedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the key that was evicted
        /// </summary>
        public string Key { get; set; }
    }
}
