using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class StreamingContextMIH
    {
        public static ConstructorInfo Constructor()
        {
            return typeof(StreamingContext).GetConstructor(new[] { typeof(StreamingContextStates) });
        }
    }
}
