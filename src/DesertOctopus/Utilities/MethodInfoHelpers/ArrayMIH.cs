using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for array MethodInfo
    /// </summary>
    internal static class ArrayMih
    {
        /// <summary>
        /// Calls Array.CreateInstance
        /// </summary>
        /// <returns>The method info for Array.CreateInstance</returns>
        public static MethodInfo CreateInstance()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(Array), nameof(Array.CreateInstance), typeof(Type), typeof(int[]));
        }

        /// <summary>
        /// Calls Array.SetValue
        /// </summary>
        /// <returns>The method info for Array.SetValue</returns>
        public static MethodInfo SetValueRank()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(Array), nameof(Array.SetValue), typeof(object), typeof(int[]));
        }

        /// <summary>
        /// Calls Array.GetValue
        /// </summary>
        /// <returns>The method info for Array.GetValue</returns>
        public static MethodInfo GetValueRank()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(Array), nameof(Array.GetValue), typeof(int[]));
        }

        /// <summary>
        /// Calls Array.GetLength
        /// </summary>
        /// <returns>The method info for Array.GetLength</returns>
        public static MethodInfo GetLength()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(Array), nameof(Array.GetLength), typeof(int));
        }

        /// <summary>
        /// Calls Array.SetValue
        /// </summary>
        /// <returns>The method info for Array.SetValue</returns>
        public static MethodInfo SetValue()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(Array), nameof(Array.SetValue), typeof(object), typeof(int));
        }

        /// <summary>
        /// Calls Array.GetValue
        /// </summary>
        /// <returns>The method info for Array.GetValue</returns>
        public static MethodInfo GetValue()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(Array), nameof(Array.GetValue), typeof(int));
        }

        /// <summary>
        /// Calls Array.Clone
        /// </summary>
        /// <returns>The method info for Array.Clone</returns>
        public static MethodInfo Clone()
        {
            return ReflectionHelpers.GetPublicStaticMethod(typeof(Array), nameof(Array.Clone));
        }
    }
}
