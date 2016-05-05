using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class EndOfStreamExceptionMIH
    {
        public static ConstructorInfo Constructor()
        {
            return typeof(EndOfStreamException).GetConstructor(new Type[0]);
        }
    }
}
