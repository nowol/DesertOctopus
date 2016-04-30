using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class DecimalMIH
    {
        public static MethodInfo GetBits()
        {
            return typeof(Decimal).GetMethod("GetBits", BindingFlags.Static | BindingFlags.Public);
        }
    }
}
