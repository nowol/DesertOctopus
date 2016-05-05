using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class QueryableMIH
    {
        public static MethodInfo AsQueryable(Type elementType)
        {
            return typeof(Queryable).GetMethod("AsQueryable", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IEnumerable<>).MakeGenericType(elementType) }, new ParameterModifier[0]);
        }
    }
}
