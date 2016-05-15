using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for InvalidOperationException MethodInfo
    /// </summary>
    internal static class InvalidOperationExceptionMIH
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
