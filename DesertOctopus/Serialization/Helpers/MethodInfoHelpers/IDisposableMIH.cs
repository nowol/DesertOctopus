using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers.MethodInfoHelpers
{
    internal static class IDisposableMIH
    {
        public static MethodInfo Dispose()
        {
            return typeof(IDisposable).GetMethod("Dispose");
        }
    }
}
