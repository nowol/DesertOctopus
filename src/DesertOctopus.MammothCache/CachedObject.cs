using System;
using System.Linq;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Cached object
    /// </summary>
    public sealed class CachedObject
    {
        /// <summary>
        /// Gets or sets the size of the object
        /// </summary>
        public int ObjectSize { get; set; }

        /// <summary>
        /// Gets or sets the value of the cached object
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the key of the cached object
        /// </summary>
        public string Key { get; set; }
    }
}