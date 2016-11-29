using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Encoding MethodInfo
    /// </summary>
    internal static class EncodingMih
    {
        /// <summary>
        /// Calls Encoding.GetByteCount
        /// </summary>
        /// <returns>The method info for Encoding.GetByteCount</returns>
        public static MethodInfo GetByteCount()
        {
            return typeof(Encoding).GetMethod(nameof(Encoding.GetByteCount), new[] { typeof(string) });
        }

        /// <summary>
        /// Calls Encoding.GetBytes
        /// </summary>
        /// <returns>The method info for Encoding.GetBytes</returns>
        public static MethodInfo GetBytes()
        {
            return typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new Type[] { typeof(string), typeof(int), typeof(int), typeof(byte[]), typeof(int) });
        }
    }
}
