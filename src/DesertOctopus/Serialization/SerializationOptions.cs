using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Class to hold the serialization options
    /// </summary>
    public class SerializationOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the name of the root object is written to the stream
        /// </summary>
        public bool OmitRootTypeName { get; set; }
    }
}
