using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class IEnumeratorMIH
    {
        public static MethodInfo MoveNext()
        {
            return typeof(IEnumerator).GetMethod("MoveNext");
        }
    }
}
