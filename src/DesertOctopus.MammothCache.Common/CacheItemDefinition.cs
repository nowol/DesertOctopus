using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// This class is used to identify and fetch an object from the cache
    /// </summary>
    public class CacheItemDefinition
    {
        /// <summary>
        /// Gets or sets the key of the object
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the optional time to live
        /// </summary>
        public TimeSpan? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the optional cargo of the key.  This can be used to store a primary key for later retrieval.
        /// </summary>
        public object Cargo { get; set; }

        /// <summary>
        /// Clone the <see cref="CacheItemDefinition"/>
        /// </summary>
        /// <returns>Cloned <see cref="CacheItemDefinition"/></returns>
        public CacheItemDefinition Clone()
        {
            return (CacheItemDefinition)this.MemberwiseClone();
        }
    }
}
