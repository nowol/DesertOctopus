using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Dictionary MethodInfo
    /// </summary>
    internal static class DictionaryMIH
    {
        /// <summary>
        /// Calls Dictionary.Add
        /// </summary>
        /// <typeparam name="TKey">TKey can be any type</typeparam>
        /// <typeparam name="TValue">TValue can be any type</typeparam>
        /// <returns>The method info for Dictionary.Add</returns>
        public static MethodInfo Add<TKey, TValue>()
        {
            return typeof(IDictionary<TKey, TValue>).GetMethod("Add", new[] { typeof(TKey), typeof(TValue) });
        }
    }
}
