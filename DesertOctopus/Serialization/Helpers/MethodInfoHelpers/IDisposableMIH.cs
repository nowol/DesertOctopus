using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class IDisposableMIH
    {
        public static MethodInfo Dispose()
        {
            return typeof(IDisposable).GetMethod("Dispose");
        }
    }
}
