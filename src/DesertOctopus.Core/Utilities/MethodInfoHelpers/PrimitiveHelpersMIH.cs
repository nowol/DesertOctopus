using System;
using System.Linq;
using System.Reflection;
using DesertOctopus.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for PrimitiveHelpers MethodInfo
    /// </summary>
    internal static class PrimitiveHelpersMih
    {
        /// <summary>
        /// Calls PrimitiveHelpers.GetLongFromDouble
        /// </summary>
        /// <returns>The method info for PrimitiveHelpers.GetLongFromDouble</returns>
        public static MethodInfo GetLongFromDouble()
        {
            return typeof(PrimitiveHelpers).GetMethod(nameof(PrimitiveHelpers.GetLongFromDouble), BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Calls PrimitiveHelpers.GetDoubleFromLong
        /// </summary>
        /// <returns>The method info for PrimitiveHelpers.GetDoubleFromLong</returns>
        public static MethodInfo GetDoubleFromLong()
        {
            return typeof(PrimitiveHelpers).GetMethod(nameof(PrimitiveHelpers.GetDoubleFromLong), BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Calls PrimitiveHelpers.GetUintFromSingle
        /// </summary>
        /// <returns>The method info for PrimitiveHelpers.GetUintFromSingle</returns>
        public static MethodInfo GetUintFromSingle()
        {
            return typeof(PrimitiveHelpers).GetMethod(nameof(PrimitiveHelpers.GetUintFromSingle), BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Calls PrimitiveHelpers.GetSingleFromUint
        /// </summary>
        /// <returns>The method info for PrimitiveHelpers.GetSingleFromUint</returns>
        public static MethodInfo GetSingleFromUint()
        {
            return typeof(PrimitiveHelpers).GetMethod(nameof(PrimitiveHelpers.GetSingleFromUint), BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
