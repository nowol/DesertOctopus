using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for NotSupportedException MethodInfo
    /// </summary>
    internal static class NotSupportedExceptionMIH
    {
        /// <summary>
        /// Calls NotSupportedException.Constructor
        /// </summary>
        /// <returns>The method info for NotSupportedException.Constructor</returns>
        public static ConstructorInfo ConstructorString()
        {
            return typeof(NotSupportedException).GetConstructor(new[] { typeof(string) });
        }
    }
}
