using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class NotSupportedExceptionMIH
    {
        public static ConstructorInfo ConstructorString()
        {
            return typeof(NotSupportedException).GetConstructor(new[] { typeof(string) });
        }
    }
}
