using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for ISerializable MethodInfo
    /// </summary>
    internal static class ISerializableMIH
    {
        /// <summary>
        /// Calls ISerializable.GetObjectData
        /// </summary>
        /// <returns>The method info for ISerializable.GetObjectData</returns>
        public static MethodInfo GetObjectData()
        {
            return typeof(ISerializable).GetMethod(nameof(ISerializable.GetObjectData), new[] { typeof(SerializationInfo), typeof(StreamingContext) });
        }
    }
}
