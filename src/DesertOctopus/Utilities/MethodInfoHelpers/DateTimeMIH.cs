using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for DateTime MethodInfo
    /// </summary>
    internal static class DateTimeMIH
    {
        /// <summary>
        /// Calls DateTime.ToBinary
        /// </summary>
        /// <returns>The method info for DateTime.ToBinary</returns>
        public static MethodInfo ToBinary()
        {
            return typeof(DateTime).GetMethod("ToBinary");
        }

        /// <summary>
        /// Calls DateTime.FromBinary
        /// </summary>
        /// <returns>The method info for DateTime.FromBinary</returns>
        public static MethodInfo FromBinary()
        {
            return typeof(DateTime).GetMethod("FromBinary", BindingFlags.Static | BindingFlags.Public);
        }
    }
}
