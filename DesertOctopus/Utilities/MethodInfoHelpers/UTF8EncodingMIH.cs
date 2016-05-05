using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DesertOctopus.Utilities
{
    internal static class UTF8EncodingMIH
    {
        public static MethodInfo GetString()
        {
            return typeof(UTF8Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) });
        }
    }
}
