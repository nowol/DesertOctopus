using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for SerializedTypeResolver MethodInfo
    /// </summary>
    internal static class SerializedTypeResolverMih
    {
        /// <summary>
        /// Calls SerializedTypeResolver.GetTypeFromFullName
        /// </summary>
        /// <returns>The method info for SerializedTypeResolver.GetTypeFromFullName</returns>
        public static MethodInfo GetTypeFromFullName_Type()
        {
            return typeof(SerializedTypeResolver).GetMethod(nameof(SerializedTypeResolver.GetTypeFromFullName), BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, new Type[] { typeof(Type) }, null);
        }

        /// <summary>
        /// Calls SerializedTypeResolver.GetTypeFromFullName
        /// </summary>
        /// <returns>The method info for SerializedTypeResolver.GetTypeFromFullName</returns>
        public static MethodInfo GetTypeFromFullName_String()
        {
            return typeof(SerializedTypeResolver).GetMethod(nameof(SerializedTypeResolver.GetTypeFromFullName), BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, new Type[] { typeof(string) }, null);
        }

        /// <summary>
        /// Calls SerializedTypeResolver.GetHashCodeFromType
        /// </summary>
        /// <returns>The method info for SerializedTypeResolver.GetHashCodeFromType</returns>
        public static MethodInfo GetHashCodeFromType()
        {
            return typeof(SerializedTypeResolver).GetMethod(nameof(SerializedTypeResolver.GetHashCodeFromType), new[] { typeof(Type) });
        }

        /// <summary>
        /// Calls SerializedTypeResolver.GetShortNameFromType
        /// </summary>
        /// <returns>The method info for SerializedTypeResolver.GetShortNameFromType</returns>
        public static MethodInfo GetShortNameFromType()
        {
            return typeof(SerializedTypeResolver).GetMethod(nameof(SerializedTypeResolver.GetShortNameFromType), new[] { typeof(Type) });
        }
    }
}
