using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Utilities
{
    internal static class ExtensionMethods
    {
        internal static bool IsStruct(this Type type)
        {
            return type.GetTypeInfo().IsValueType && !type.GetTypeInfo().IsEnum && !type.GetTypeInfo().IsPrimitive;
        }
    }
}
