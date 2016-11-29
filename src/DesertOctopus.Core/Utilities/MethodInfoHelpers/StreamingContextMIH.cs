#if NET452 || NETSTANDARD2_0

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
            return typeof(StreamingContext).GetConstructor(new[] { typeof(StreamingContextStates) });
        }
    }
}

#endif