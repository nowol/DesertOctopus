using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for TimeSpan MethodInfo
    /// </summary>
    internal static class TimeSpanMih
    {
        /// <summary>
        /// Calls TimeSpan.FromTicks
        /// </summary>
        /// <returns>The method info for TimeSpan.FromTicks</returns>
        public static MethodInfo FromTicks()
        {
            return typeof(TimeSpan).GetMethod(nameof(TimeSpan.FromTicks), BindingFlags.Static | BindingFlags.Public);
        }
    }
}
