using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for EndOfStreamException MethodInfo
    /// </summary>
    internal static class EndOfStreamExceptionMIH
    {
        /// <summary>
        /// Calls EndOfStreamException.Constructor
        /// </summary>
        /// <returns>The method info for EndOfStreamException.Constructor</returns>
        public static ConstructorInfo Constructor()
        {
            return typeof(EndOfStreamException).GetConstructor(new Type[0]);
        }
    }
}
