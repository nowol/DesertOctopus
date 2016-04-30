using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class InternalSerializationStuff
    {
        public const short Version = 1;

        public enum SerializationType : byte
        {
            ValueType = 0,
            Class = 1
        }

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var fields = new List<FieldInfo>();
            var targetType = type;

            do
            {
                fields.AddRange(targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                          .Where(fi => (fi.Attributes & FieldAttributes.NotSerialized) == 0));
                targetType = targetType.BaseType;
            } while (targetType != null); 


            return fields.OrderBy(f => f.Name, StringComparer.Ordinal);
        }
    }
}