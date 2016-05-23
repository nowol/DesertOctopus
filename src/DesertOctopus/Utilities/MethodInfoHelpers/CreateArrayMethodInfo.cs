using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for array creation MethodInfo
    /// </summary>
    internal static class CreateArrayMethodInfo
    {
        private static readonly MethodInfo CreateArrayMethod = typeof(CreateArrayMethodInfo).GetMethod("CreateArray", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo ArrayLengthMethod = typeof(Array).GetMethod("GetLength");
        private static readonly MethodInfo CreateArrayWithLengthMethod = typeof(CreateArrayMethodInfo).GetMethod("CreateArrayWithLength", BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// Calls CreateArrayMethodInfo.CreateArray
        /// </summary>
        /// <param name="elementType">Type or array to create</param>
        /// <returns>The method info for CreateArrayMethodInfo.CreateArray</returns>
        public static MethodInfo GetCreateArrayMethodInfo(Type elementType)
        {
            // to do create a cache of 'array creator'
            return CreateArrayMethod.MakeGenericMethod(elementType);
        }

        /// <summary>
        /// Create an array of type and length
        /// </summary>
        /// <param name="elementType">Type of the array</param>
        /// <param name="length">Capacity of the array</param>
        /// <returns>The method info for Array.SetValue</returns>
        public static object CreateArrayWithLength(Type elementType, int length)
        {
            // to do create a cache of 'array creator'
            return GetCreateArrayMethodInfo(elementType).Invoke(null, new object[] { length });
        }

        /// <summary>
        /// Calls Array.SetValue
        /// </summary>
        /// <returns>The method info for Array.SetValue</returns>
        public static MethodInfo GetArrayLengthMethod()
        {
            return ArrayLengthMethod;
        }

        /// <summary>
        /// Create an array of type and length
        /// </summary>
        /// <typeparam name = "T" > Any reference type</typeparam>
        /// <param name="length">Capacity of the array</param>
        /// <returns>The method info for Array.SetValue</returns>
        public static T[] CreateArray<T>(int length)
        {
            return new T[length];
        }

        /// <summary>
        /// Calls CreateArrayMethodInfo.CreateArrayWithLengthMethod
        /// </summary>
        /// <param name="typeExpr">Type of the array</param>
        /// <param name="length">Capacity of the array</param>
        /// <returns>Expression for CreateArrayMethodInfo.CreateArrayWithLengthMethod</returns>
        internal static Expression CreateArray(ParameterExpression typeExpr, ParameterExpression length)
        {
            return Expression.Call(CreateArrayWithLengthMethod, typeExpr, length);
        }
    }
}