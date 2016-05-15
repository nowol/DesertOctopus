using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for IEnumerator MethodInfo
    /// </summary>
    internal static class IEnumeratorMIH
    {
        /// <summary>
        /// Calls IEnumerator.MoveNext
        /// </summary>
        /// <returns>The method info for IEnumerator.MoveNext</returns>
        public static MethodInfo MoveNext()
        {
            return typeof(IEnumerator).GetMethod("MoveNext");
        }
    }
}
