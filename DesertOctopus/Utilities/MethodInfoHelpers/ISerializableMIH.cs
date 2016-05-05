using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    internal static class ISerializableMIH
    {
        public static MethodInfo GetObjectData()
        {
            return typeof(ISerializable).GetMethod("GetObjectData", new[] { typeof(SerializationInfo), typeof(StreamingContext) });
        }
    }
}
