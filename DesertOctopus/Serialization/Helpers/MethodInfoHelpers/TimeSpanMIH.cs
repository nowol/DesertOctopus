using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class TimeSpanMIH
    {
        public static MethodInfo FromTicks()
        {
            return typeof(TimeSpan).GetMethod("FromTicks", BindingFlags.Static | BindingFlags.Public);
        }
    }
}
