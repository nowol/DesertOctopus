using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DesertOctopus.Polyfills;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for EndOfStreamException MethodInfo
    /// </summary>
    internal static class EndOfStreamExceptionMih
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
