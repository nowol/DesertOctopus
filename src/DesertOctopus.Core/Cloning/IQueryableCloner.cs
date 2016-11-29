using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DesertOctopus.Polyfills;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Helper class for IQueryable
    /// </summary>
    internal static class IQueryableCloner
    {
        /// <summary>
        /// Detect if the type implements the expected generic interface
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="expectedInterface">Expected interface</param>
        /// <returns>The expected generic type if found</returns>
        internal static Type GetInterfaceType(Type type, Type expectedInterface)
        {
            Type enumerableInterface = null;
            if (type.GetTypeInfo().IsGenericType
                && type.GetGenericTypeDefinition() == expectedInterface)
            {
                enumerableInterface = type;
            }
            else
            {
                enumerableInterface = type.GetTypeInfo().ImplementedInterfaces
                                                .FirstOrDefault(t => t.GetTypeInfo().IsGenericType
                                                                     && t.GetGenericTypeDefinition() == expectedInterface
                                                                     && t.GetTypeInfo().GenericTypeArguments.Length == 1);
            }

            return enumerableInterface;
        }

        /// <summary>
        /// Detect if the specified type is an IQueryable&lt;&gt;
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>True is type is  an IQueryable&lt;&gt;</returns>
        internal static bool IsGenericIQueryableType(Type type)
        {
            return GetInterfaceType(type, typeof(IQueryable<>)) != null;
        }

        /// <summary>
        /// Convert the obj to IQueryable if it's a generic list
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <returns>Converted object</returns>
        internal static object ConvertToNonGenericQueryable(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var objectType = obj.GetType();

            if (objectType != typeof(string)
                && obj is IEnumerable)
            {
                var asq = typeof(Queryable).GetMethod("AsQueryable", isStatic: true, parameters: new[] { typeof(IEnumerable) });
                return asq.Invoke(null, new[] { obj });
            }

            return obj;
        }
    }
}
