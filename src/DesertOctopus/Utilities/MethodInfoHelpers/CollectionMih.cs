using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for ICollection MethodInfo
    /// </summary>
    internal static class CollectionMih
    {
        /// <summary>
        /// Calls ICollection.Count
        /// </summary>
        /// <typeparam name="T">Any types</typeparam>
        /// <returns>The method info for ICollection.Count</returns>
        public static PropertyInfo Count<T>()
        {
            return typeof(ICollection<T>).GetProperty(nameof(ICollection<T>.Count));
        }
    }
}
