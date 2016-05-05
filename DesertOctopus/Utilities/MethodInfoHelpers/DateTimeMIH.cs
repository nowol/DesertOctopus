using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class DateTimeMIH
    {
        public static MethodInfo ToBinary()
        {
            return typeof(DateTime).GetMethod("ToBinary");
        }

        public static MethodInfo FromBinary()
        {
            return typeof(DateTime).GetMethod("FromBinary", BindingFlags.Static | BindingFlags.Public);
        }
    }
}
