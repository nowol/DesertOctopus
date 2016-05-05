using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class ObjectMIH
    {
        public static MethodInfo GetTypeMethod()
        {
            return typeof(object).GetMethod("GetType", new Type[0]);
        }
    }
}
