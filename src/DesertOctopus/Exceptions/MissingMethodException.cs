using System;
using System.Runtime.Serialization;

namespace DesertOctopus.Exceptions
{
    /// <summary>
    /// Exception used when a specific method was not found
    /// </summary>
#if !NETSTANDARD1_6
    [Serializable]
#endif
    public class MissingMethodException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingMethodException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public MissingMethodException(string message)
            : base(message)
        {
        }

#if !NETSTANDARD1_6
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingMethodException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected MissingMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}