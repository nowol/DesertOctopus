using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class SerializedTypeResolverMIH
    {
        public static MethodInfo GetTypeFromFullName_Type()
        {
            return typeof(SerializedTypeResolver).GetMethod("GetTypeFromFullName", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, new Type[] { typeof(Type) }, null);
        }

        public static MethodInfo GetTypeFromFullName_String()
        {
            return typeof(SerializedTypeResolver).GetMethod("GetTypeFromFullName", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, new Type[] { typeof(string) }, null);
        }

        public static MethodInfo GetHashCodeFromType()
        {
            return typeof(SerializedTypeResolver).GetMethod("GetHashCodeFromType", new[] { typeof(Type) });
        }

        public static MethodInfo GetShortNameFromType()
        {
            return typeof(SerializedTypeResolver).GetMethod("GetShortNameFromType", new[] { typeof(Type) });
        }
    }
}
