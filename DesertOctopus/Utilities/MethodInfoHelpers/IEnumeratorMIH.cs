using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class IEnumeratorMIH
    {
        public static MethodInfo MoveNext()
        {
            return typeof(IEnumerator).GetMethod("MoveNext");
        }
    }
}
