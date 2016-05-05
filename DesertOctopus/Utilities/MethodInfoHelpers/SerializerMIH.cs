using System;
using System.Linq;
using System.Reflection;
using DesertOctopus.Serialization;

namespace DesertOctopus.Utilities
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
            return typeof(ObjectCleaner).GetMethod("PrepareObjectForSerialization", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public static MethodInfo GetTypeSerializer()
        {
            return typeof(Serializer).GetMethod("GetTypeSerializer", BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
