using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class InvalidOperationExceptionMIH
    {
        public static ConstructorInfo Constructor()
        {
            return typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
        }
    }
}
