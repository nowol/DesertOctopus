using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Exceptions
{
    /// <summary>
    /// Exception thrown when the version is not the expected one
    /// </summary>
    [Serializable]
    public class InvalidSerializationVersionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSerializationVersionException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public InvalidSerializationVersionException(string message)
            : base(message)
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSerializationVersionException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected InvalidSerializationVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
