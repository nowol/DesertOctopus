using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for IDisposable MethodInfo
    /// </summary>
    internal static class IDisposableMih
    {
        /// <summary>
        /// Calls IDisposable.Dispose
        /// </summary>
        /// <returns>The method info for IDisposable.Dispose</returns>
        public static MethodInfo Dispose()
        {
            return typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
        }
    }
}
