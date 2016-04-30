using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class UTF8EncodingMIH
    {
        public static MethodInfo GetString()
        {
            return typeof(UTF8Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) });
        }
    }
}
