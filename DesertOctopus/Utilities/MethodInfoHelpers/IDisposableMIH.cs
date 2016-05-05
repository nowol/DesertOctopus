using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class IDisposableMIH
    {
        public static MethodInfo Dispose()
        {
            return typeof(IDisposable).GetMethod("Dispose");
        }
    }
}
