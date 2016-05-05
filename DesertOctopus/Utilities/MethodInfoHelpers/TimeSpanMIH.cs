using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class TimeSpanMIH
    {
        public static MethodInfo FromTicks()
        {
            return typeof(TimeSpan).GetMethod("FromTicks", BindingFlags.Static | BindingFlags.Public);
        }
    }
}
