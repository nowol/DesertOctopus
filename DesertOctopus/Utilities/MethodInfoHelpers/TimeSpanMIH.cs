using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for TimeSpan MethodInfo
    /// </summary>
    internal static class TimeSpanMIH
    {
        /// <summary>
        /// Calls TimeSpan.FromTicks
        /// </summary>
        /// <returns>The method info for TimeSpan.FromTicks</returns>
        public static MethodInfo FromTicks()
        {
            return typeof(TimeSpan).GetMethod("FromTicks", BindingFlags.Static | BindingFlags.Public);
        }
    }
}
