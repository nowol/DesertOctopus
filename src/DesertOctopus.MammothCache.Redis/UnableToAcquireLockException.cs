using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.MammothCache.Redis
{
    /// <summary>
    /// Exception used when a Redis lock cannot be acquired
    /// </summary>
    [Serializable]
    public class UnableToAcquireLockException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnableToAcquireLockException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public UnableToAcquireLockException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnableToAcquireLockException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected UnableToAcquireLockException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
