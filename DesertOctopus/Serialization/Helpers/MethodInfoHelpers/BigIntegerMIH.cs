using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class BigIntegerMIH
    {
        public static MethodInfo ToByteArray()
        {
            return typeof(BigInteger).GetMethod("ToByteArray");
        }

        public static ConstructorInfo Constructor()
        {
            return typeof(BigInteger).GetConstructor(new[] { typeof(byte[]) });
        }
    }
}
