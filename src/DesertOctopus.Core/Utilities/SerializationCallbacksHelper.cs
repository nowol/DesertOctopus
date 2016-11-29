using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class provide access to serialization callbacks
    /// </summary>
    internal static class SerializationCallbacksHelper
    {
        /// <summary>
        /// Get all methods with the specified attribute type
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="attrType">Attribute type to find</param>
        /// <param name="parameterTypes">Parameters type</param>
        /// <returns>All methods with the specified attribute type</returns>
        internal static List<MethodInfo> GetMethodsWithAttributes(Type type, Type attrType, Type[] parameterTypes)
        {
            List<MethodInfo> methods = new List<MethodInfo>();

            var targetType = type;
            do
            {
                methods.AddRange(targetType.GetTypeInfo().DeclaredMethods
                                            .Where(m => m.GetCustomAttributes(attrType, false).Any()
                                                        && m.GetParameters().Select(x => x.ParameterType).SequenceEqual(parameterTypes))
                                            .ToList());
                targetType = targetType.GetTypeInfo().BaseType;
            }
            while (targetType != null);

            return methods;
        }

        private static IEnumerable<Expression> GenerateCallAttributeExpression<TAttrType>(Type type, Type[] parameterTypes, Expression objToSerialize, Expression context)
        {
            var methods = GetMethodsWithAttributes(type, typeof(TAttrType), parameterTypes);
            return methods.Select(m => Expression.Call(Expression.Convert(objToSerialize, type), m, context));
        }

        /// <summary>
        /// Generate a list of expression that calls methods with the OnSerializing attribute
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="context">Serialization context</param>
        /// <returns>A list of expression that calls methods with the OnSerializing attribute</returns>
        public static IEnumerable<Expression> GenerateOnSerializingAttributeExpression(Type type, Expression objToSerialize, Expression context)
        {
            return GenerateCallAttributeExpression<OnSerializingAttribute>(type, new[] { typeof(StreamingContext) }, objToSerialize, context);
        }

        /// <summary>
        /// Generate a list of expression that calls methods with the OnSerialized attribute
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="context">Serialization context</param>
        /// <returns>A list of expression that calls methods with the OnSerialized attribute</returns>
        public static IEnumerable<Expression> GenerateOnSerializedAttributeExpression(Type type, Expression objToSerialize, Expression context)
        {
            return GenerateCallAttributeExpression<OnSerializedAttribute>(type, new[] { typeof(StreamingContext) }, objToSerialize, context);
        }

        /// <summary>
        /// Generate a list of expression that calls methods with the OnDeserializing attribute
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="newInstance">Deserialized object</param>
        /// <param name="context">Serialization context</param>
        /// <returns>A list of expression that calls methods with the OnDeserializing attribute</returns>
        public static IEnumerable<Expression> GenerateOnDeserializingAttributeExpression(Type type, Expression newInstance, Expression context)
        {
            return GenerateCallAttributeExpression<OnDeserializingAttribute>(type, new[] { typeof(StreamingContext) }, newInstance, context);
        }

        /// <summary>
        /// Generate a list of expression that calls methods with the OnDeserialized attribute
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="newInstance">Deserialized object</param>
        /// <param name="context">Serialization context</param>
        /// <returns>A list of expression that calls methods with the OnDeserialized attribute</returns>
        public static IEnumerable<Expression> GenerateOnDeserializedAttributeExpression(Type type, Expression newInstance, Expression context)
        {
            return GenerateCallAttributeExpression<OnDeserializedAttribute>(type, new[] { typeof(StreamingContext) }, newInstance, context);
        }

#if NET452 || NETSTANDARD2_0
        /// <summary>
        /// Generate a list of expression that calls methods with the OnDeserialization attribute
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="newInstance">Deserialized object</param>
        /// <returns>A list of expression that calls methods with the OnDeserialization attribute</returns>
        public static Expression GenerateCallIDeserializationExpression(Type type, ParameterExpression newInstance)
        {
            if (typeof(IDeserializationCallback).IsAssignableFrom(type))
            {
                return Expression.Call(Expression.Convert(newInstance, typeof(IDeserializationCallback)), typeof(IDeserializationCallback).GetMethod("OnDeserialization", new[] { typeof(object) }), newInstance);
            }

            return Expression.Empty();
        }
#endif
    }
}