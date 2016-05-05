using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class DecimalMIH
    {
        public static MethodInfo GetBits()
        {
            return typeof(Decimal).GetMethod("GetBits", BindingFlags.Static | BindingFlags.Public);
        }
    }
}
