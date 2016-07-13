using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Queryable MethodInfo
    /// </summary>
    internal static class QueryableMIH
    {
        /// <summary>
        /// Calls Queryable.AsQueryable
        /// </summary>
        /// <param name="elementType">Type used for AsQueryable</param>
        /// <returns>The method info for Queryable.AsQueryable</returns>
        public static MethodInfo AsQueryable(Type elementType)
        {
            return typeof(Queryable).GetMethod(nameof(Queryable.AsQueryable), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IEnumerable<>).MakeGenericType(elementType) }, new ParameterModifier[0]);
        }
    }
}
