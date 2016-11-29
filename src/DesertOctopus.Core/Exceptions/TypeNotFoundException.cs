using System;
using System.Runtime.Serialization;

namespace DesertOctopus.Exceptions
{
    /// <summary>
    /// Exception used when a type cannot be found
    /// </summary>
    [Serializable]
    public class TypeNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public TypeNotFoundException(string message)
            : base(message)
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotFoundException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected TypeNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}