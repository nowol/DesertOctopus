using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Result of a GetTimeToLive operation
    /// </summary>
    public class TimeToLiveResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the key exists or not
        /// </summary>
        public bool KeyExists { get; set; }

        /// <summary>
        /// Gets or sets the remaining TTL of the key.  Null means no expiration
        /// </summary>
        public TimeSpan? TimeToLive { get; set; }
    }
}
