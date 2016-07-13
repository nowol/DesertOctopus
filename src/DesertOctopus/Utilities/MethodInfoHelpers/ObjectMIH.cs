using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for object MethodInfo
    /// </summary>
    internal static class ObjectMih
    {
        /// <summary>
        /// Calls object.GetType
        /// </summary>
        /// <returns>The method info for object.GetType</returns>
        public static MethodInfo GetTypeMethod()
        {
            return typeof(object).GetMethod(nameof(object.GetType), new Type[0]);
        }
    }
}
