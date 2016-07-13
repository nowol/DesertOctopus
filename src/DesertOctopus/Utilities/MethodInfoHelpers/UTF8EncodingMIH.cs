using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for UTF8Encoding MethodInfo
    /// </summary>
    internal static class Utf8EncodingMih
    {
        /// <summary>
        /// Calls UTF8Encoding.GetString
        /// </summary>
        /// <returns>The method info for UTF8Encoding.GetString</returns>
        public static MethodInfo GetString()
        {
            return typeof(UTF8Encoding).GetMethod(nameof(UTF8Encoding.GetString), new Type[] { typeof(byte[]) });
        }
    }
}
