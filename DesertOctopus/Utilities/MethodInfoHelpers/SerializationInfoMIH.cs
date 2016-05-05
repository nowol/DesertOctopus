using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    internal static class SerializationInfoMIH
    {
        public static ConstructorInfo Constructor()
        {
            return typeof(SerializationInfo).GetConstructor(new[] { typeof(Type), typeof(FormatterConverter) });
        }

        public static MethodInfo AddValue()
        {
            return typeof(SerializationInfo).GetMethod("AddValue", new[] { typeof(string), typeof(object) });
        }

        public static PropertyInfo MemberCount()
        {
            return typeof(SerializationInfo).GetProperty("MemberCount");
        }

        public static MethodInfo GetEnumerator()
        {
            return typeof(SerializationInfo).GetMethod("GetEnumerator", BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public, null, new Type[0], new ParameterModifier[0]);
        }
    }
}
