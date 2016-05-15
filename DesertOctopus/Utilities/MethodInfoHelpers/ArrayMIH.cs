using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for array MethodInfo
    /// </summary>
    internal static class ArrayMIH
    {
        /// <summary>
        /// Calls Array.CreateInstance
        /// </summary>
        /// <returns>The method info for Array.CreateInstance</returns>
        public static MethodInfo CreateInstance()
        {
            return typeof(Array).GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Type), typeof(int[]) }, null);
        }

        /// <summary>
        /// Calls Array.SetValue
        /// </summary>
        /// <returns>The method info for Array.SetValue</returns>
        public static MethodInfo SetValueRank()
        {
            return typeof(Array).GetMethod("SetValue", new[] { typeof(object), typeof(int[]) });
        }

        /// <summary>
        /// Calls Array.GetValue
        /// </summary>
        /// <returns>The method info for Array.GetValue</returns>
        public static MethodInfo GetValueRank()
        {
            return typeof(Array).GetMethod("GetValue", new[] { typeof(int[]) });
        }

        /// <summary>
        /// Calls Array.GetLength
        /// </summary>
        /// <returns>The method info for Array.GetLength</returns>
        public static MethodInfo GetLength()
        {
            return typeof(Array).GetMethod("GetLength");
        }

        /// <summary>
        /// Calls Array.SetValue
        /// </summary>
        /// <returns>The method info for Array.SetValue</returns>
        public static MethodInfo SetValue()
        {
            return typeof(Array).GetMethod("SetValue", new[] { typeof(object), typeof(int) });
        }

        /// <summary>
        /// Calls Array.GetValue
        /// </summary>
        /// <returns>The method info for Array.GetValue</returns>
        public static MethodInfo GetValue()
        {
            return typeof(Array).GetMethod("GetValue", new[] { typeof(int) });
        }

        /// <summary>
        /// Calls Array.Clone
        /// </summary>
        /// <returns>The method info for Array.Clone</returns>
        public static MethodInfo Clone()
        {
            return typeof(Array).GetMethod("Clone");
        }
    }
}
