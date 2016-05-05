using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DesertOctopus.Utilities
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
