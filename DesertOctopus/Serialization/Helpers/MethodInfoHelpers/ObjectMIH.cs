using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class ObjectMIH
    {
        public static MethodInfo GetTypeMethod()
        {
            return typeof(object).GetMethod("GetType", new Type[0]);
        }
    }
}
