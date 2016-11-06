using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Exceptions
{
    /// <summary>
    /// Exception thrown when the "omit root type name" is not the expected one
    /// </summary>
    [Serializable]
    public class InvalidOmitRootTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOmitRootTypeException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public InvalidOmitRootTypeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOmitRootTypeException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected InvalidOmitRootTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}