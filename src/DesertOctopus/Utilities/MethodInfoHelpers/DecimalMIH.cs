using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Decimal MethodInfo
    /// </summary>
    internal static class DecimalMIH
    {
        /// <summary>
        /// Calls Decimal.GetBits
        /// </summary>
        /// <returns>The method info for Decimal.GetBits</returns>
        public static MethodInfo GetBits()
        {
            return typeof(decimal).GetMethod(nameof(decimal.GetBits), BindingFlags.Static | BindingFlags.Public);
        }
    }
}
