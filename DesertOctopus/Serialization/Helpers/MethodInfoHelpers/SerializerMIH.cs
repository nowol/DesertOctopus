using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class SerializerMIH
    {
        public static MethodInfo ConvertEnumerableToArray(Type itemType)
        {
            return typeof(Serializer).GetMethod("ConvertEnumerableToArray",
                                                BindingFlags.Static | BindingFlags.NonPublic)
                                     .MakeGenericMethod(itemType);
        }

        public static MethodInfo PrepareObjectForSerialization()
        {
            return typeof(Serializer).GetMethod("PrepareObjectForSerialization", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static MethodInfo GetTypeSerializer()
        {
            return typeof(Serializer).GetMethod("GetTypeSerializer", BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
