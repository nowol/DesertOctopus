using System;
using System.Linq;
using System.Reflection;
using DesertOctopus.Polyfills;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for InvalidOperationException MethodInfo
    /// </summary>
    internal static class InvalidOperationExceptionMih
    {
        /// <summary>
        /// Calls InvalidOperationException.Constructor
        /// </summary>
        /// <returns>The method info for InvalidOperationException.Constructor</returns>
        public static ConstructorInfo Constructor()
        {
            return typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
        }
    }
}
