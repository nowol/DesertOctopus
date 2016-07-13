using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for IEnumerable MethodInfo
    /// </summary>
    internal static class IEnumerableMih
    {
        /// <summary>
        /// Calls IEnumerable.SetValue
        /// </summary>
        /// <typeparam name="TKey">TKey can be any type</typeparam>
        /// <typeparam name="TValue">TValue can be any type</typeparam>
        /// <returns>The method info for IEnumerable.GetEnumerator</returns>
        public static MethodInfo GetEnumerator<TKey, TValue>()
        {
            return typeof(IEnumerable<KeyValuePair<TKey, TValue>>).GetMethod(nameof(IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator), new Type[0]);
        }
    }
}
