using System;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for BigInteger MethodInfo
    /// </summary>
    internal static class BigIntegerMIH
    {
        /// <summary>
        /// Calls BigInteger.ToByteArray
        /// </summary>
        /// <returns>The method info for BigInteger.ToByteArray</returns>
        public static MethodInfo ToByteArray()
        {
            return typeof(BigInteger).GetMethod(nameof(BigInteger.ToByteArray));
        }

        /// <summary>
        /// Calls BigInteger.GetConstructor(byte[])
        /// </summary>
        /// <returns>The method info for BigInteger.GetConstructor(byte[])</returns>
        public static ConstructorInfo Constructor()
        {
            return typeof(BigInteger).GetConstructor(new[] { typeof(byte[]) });
        }
    }
}
