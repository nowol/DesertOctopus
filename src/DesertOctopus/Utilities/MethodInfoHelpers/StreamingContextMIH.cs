using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for StreamingContext MethodInfo
    /// </summary>
    internal static class StreamingContextMih
    {
        /// <summary>
        /// Calls StreamingContext.Constructor
        /// </summary>
        /// <returns>The method info for StreamingContext.Constructor</returns>
        public static ConstructorInfo Constructor()
        {
            return ReflectionHelpers.GetPublicConstructor(typeof(StreamingContext), typeof(StreamingContextStates));
        }
    }
}
