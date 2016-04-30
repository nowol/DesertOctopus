using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class PrimitiveHelpersMIH
    {
        public static MethodInfo GetLongFromDouble()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetLongFromDouble", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static MethodInfo GetDoubleFromLong()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetDoubleFromLong", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static MethodInfo GetUintFromSingle()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetUintFromSingle", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static MethodInfo GetSingleFromUint()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetSingleFromUint", BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
