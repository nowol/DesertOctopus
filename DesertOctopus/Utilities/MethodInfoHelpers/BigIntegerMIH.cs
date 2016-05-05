using System;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace DesertOctopus.Utilities
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
