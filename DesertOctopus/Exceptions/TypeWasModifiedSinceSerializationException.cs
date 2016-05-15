using System;
using System.Reflection;
using System.Runtime.Serialization;
using DesertOctopus.Utilities;

namespace DesertOctopus.Exceptions
{
    /// <summary>
    /// Exception used when a type was modified since it was serialized
    /// </summary>
    [Serializable]
    public class TypeWasModifiedSinceSerializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWasModifiedSinceSerializationException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception</param>
        internal TypeWasModifiedSinceSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWasModifiedSinceSerializationException"/> class.
        /// </summary>
        /// <param name="type">Type with the issue</param>
        internal TypeWasModifiedSinceSerializationException(TypeWithHashCode type)
            : base(type.Type.ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWasModifiedSinceSerializationException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected TypeWasModifiedSinceSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns the constructor that takes a Type parameter
        /// </summary>
        /// <returns>The constructor that takes a Type parameter</returns>
        internal static ConstructorInfo GetConstructor()
        {
            return typeof(TypeWasModifiedSinceSerializationException).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                                                     null,
                                                                                     new[]
                                                                                     {
                                                                                         typeof(TypeWithHashCode)
                                                                                     },
                                                                                     new ParameterModifier[0]);
        }
    }
}
