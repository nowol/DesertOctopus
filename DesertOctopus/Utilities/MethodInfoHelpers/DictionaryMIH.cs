using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class DictionaryMIH
    {
        public static MethodInfo Add<TKey, TValue>()
        {
            return typeof(IDictionary<TKey, TValue>).GetMethod("Add", new[] { typeof(TKey), typeof(TValue) });
        }
    }
}
