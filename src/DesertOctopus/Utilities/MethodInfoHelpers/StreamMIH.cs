using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Stream MethodInfo
    /// </summary>
    internal static class StreamMih
    {
        /// <summary>
        /// Calls Stream.WriteByte
        /// </summary>
        /// <returns>The method info for Stream.WriteByte</returns>
        public static MethodInfo WriteByte()
        {
            return typeof(System.IO.Stream).GetMethod(nameof(System.IO.Stream.WriteByte));
        }

        /// <summary>
        /// Calls Stream.Write
        /// </summary>
        /// <returns>The method info for Stream.Write</returns>
        public static MethodInfo Write()
        {
            return typeof(System.IO.Stream).GetMethod(nameof(System.IO.Stream.Write));
        }

        /// <summary>
        /// Calls Stream.ReadByte
        /// </summary>
        /// <returns>The method info for Stream.ReadByte</returns>
        public static MethodInfo ReadByte()
        {
            return typeof(System.IO.Stream).GetMethod(nameof(System.IO.Stream.ReadByte));
        }

        /// <summary>
        /// Calls Stream.Read
        /// </summary>
        /// <returns>The method info for Stream.Read</returns>
        public static MethodInfo Read()
        {
            return typeof(System.IO.Stream).GetMethod(nameof(System.IO.Stream.Read));
        }
    }
}
