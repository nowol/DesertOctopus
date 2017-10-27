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
            return ReflectionHelpers.GetPublicStaticMethod(typeof(SerializedTypeResolver), nameof(SerializedTypeResolver.GetTypeFromFullName), typeof(Type));
        }

        /// <summary>
        /// Calls SerializedTypeResolver.GetTypeFromFullName
        /// </summary>
        /// <returns>The method info for SerializedTypeResolver.GetTypeFromFullName</returns>
        public static MethodInfo GetTypeFromFullName_String()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(SerializedTypeResolver), nameof(SerializedTypeResolver.GetTypeFromFullName), typeof(string));
        }

        /// <summary>
        /// Calls SerializedTypeResolver.GetHashCodeFromType
        /// </summary>
        /// <returns>The method info for SerializedTypeResolver.GetHashCodeFromType</returns>
        public static MethodInfo GetHashCodeFromType()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(SerializedTypeResolver), nameof(SerializedTypeResolver.GetHashCodeFromType), typeof(Type));
        }

        /// <summary>
        /// Calls SerializedTypeResolver.GetShortNameFromType
        /// </summary>
        /// <returns>The method info for SerializedTypeResolver.GetShortNameFromType</returns>
        public static MethodInfo GetShortNameFromType()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(SerializedTypeResolver), nameof(SerializedTypeResolver.GetShortNameFromType), typeof(Type));
        }
    }
}
