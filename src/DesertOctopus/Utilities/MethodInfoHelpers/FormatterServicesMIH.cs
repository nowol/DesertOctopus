using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for FormatterServices MethodInfo
    /// </summary>
    internal static class FormatterServicesMIH
    {
        /// <summary>
        /// Calls FormatterServices.GetUninitializedObject
        /// </summary>
        /// <returns>The method info for FormatterServices.GetUninitializedObject</returns>
        public static MethodInfo GetUninitializedObject()
        {
            return typeof(FormatterServices).GetMethod(nameof(FormatterServices.GetUninitializedObject), BindingFlags.Public | BindingFlags.Static);
        }
    }
}
