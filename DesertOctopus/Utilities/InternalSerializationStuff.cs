using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class that defines some constants
    /// </summary>
    internal static class InternalSerializationStuff
    {
        /// <summary>
        /// Version of the serialization engine
        /// </summary>
        public const short Version = 1;

        /// <summary>
        /// Type of serialized object
        /// </summary>
        public enum SerializationType : byte
        {
            /// <summary>
            /// Value/Primitive type
            /// </summary>
            ValueType = 0,

            /// <summary>
            /// Reference type
            /// </summary>
            Class = 1
        }

        /// <summary>
        /// Gets the fields of a type that can be serialized
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>The fields of a type</returns>
        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var fields = new List<FieldInfo>();
            var targetType = type;

            do
            {
                fields.AddRange(targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                          .Where(fi => (fi.Attributes & FieldAttributes.NotSerialized) == 0));
                targetType = targetType.BaseType;
            }
            while (targetType != null);

            return fields.OrderBy(f => f.Name, StringComparer.Ordinal);
        }
    }
}