using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class EndOfStreamExceptionMIH
    {
        public static ConstructorInfo Constructor()
        {
            return typeof(EndOfStreamException).GetConstructor(new Type[0]);
        }
    }
}
