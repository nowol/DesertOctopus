using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for List MethodInfo
    /// </summary>
    internal static class ListMIH
    {
        /// <summary>
        /// Calls List.Add
        /// </summary>
        /// <returns>The method info for List.Add</returns>
        public static MethodInfo ObjectListAdd()
        {
            return typeof(List<object>).GetMethod(nameof(List<object>.Add), new[] { typeof(object) });
        }
    }
}
