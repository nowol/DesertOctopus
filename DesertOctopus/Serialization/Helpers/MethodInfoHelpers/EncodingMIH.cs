using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class EncodingMIH
    {
        public static MethodInfo GetByteCount()
        {
            return typeof(Encoding).GetMethod("GetByteCount", new[] { typeof(string) });
        }

        public static MethodInfo GetBytes()
        {
            return typeof(Encoding).GetMethod("GetBytes", new Type[] { typeof(string), typeof(int), typeof(int), typeof(byte[]), typeof(int) });
        }
    }
}
