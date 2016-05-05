using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class NotSupportedExceptionMIH
    {
        public static ConstructorInfo ConstructorString()
        {
            return typeof(NotSupportedException).GetConstructor(new[] { typeof(string) });
        }
    }
}
