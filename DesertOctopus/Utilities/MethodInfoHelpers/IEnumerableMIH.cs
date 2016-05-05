using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class IEnumerableMIH
    {
        public static MethodInfo GetEnumerator<TKey, TValue>()
        {
            return typeof(IEnumerable<KeyValuePair<TKey, TValue>>).GetMethod("GetEnumerator", new Type[0]);
        }
    }
}
