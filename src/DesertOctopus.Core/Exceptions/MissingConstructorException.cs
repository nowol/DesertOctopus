using System;
using System.Runtime.Serialization;

namespace DesertOctopus.Exceptions
{
    /// <summary>
    /// Exception used when a specific type constructor was not found
    /// </summary>
    [Serializable]
    public class MissingConstructorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingConstructorException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public MissingConstructorException(string message)
            : base(message)
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingConstructorException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected MissingConstructorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}