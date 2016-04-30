using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class CreateArrayMethodInfo
    {
        private static readonly MethodInfo CreateArrayMethod = typeof(CreateArrayMethodInfo).GetMethod("CreateArray", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo ArrayLengthMethod = typeof(Array).GetMethod("GetLength");
        private static readonly MethodInfo CreateArrayWithLengthMethod = typeof(CreateArrayMethodInfo).GetMethod("CreateArrayWithLength", BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo GetCreateArrayMethodInfo(Type elementType)
        {
            // to do create a cache of 'array creator'
            return CreateArrayMethod.MakeGenericMethod(elementType);
        }

        public static object CreateArrayWithLength(Type elementType, int length)
        {
            // to do create a cache of 'array creator'
            return GetCreateArrayMethodInfo(elementType).Invoke(null, new object[] { length });
        }

        public static MethodInfo GetArrayLengthMethod()
        {
            return ArrayLengthMethod;
        }

        public static T[] CreateArray<T>(int length)
        {
            return new T[length];
        }

        internal static Expression CreateArray(ParameterExpression typeExpr, ParameterExpression length)
        {
            return Expression.Call(CreateArrayWithLengthMethod, typeExpr, length);
        }
    }
}