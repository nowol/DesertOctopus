using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
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
