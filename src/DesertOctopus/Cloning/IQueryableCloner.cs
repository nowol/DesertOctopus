using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DesertOctopus.Utilities;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Helper class for IQueryable
    /// </summary>
    internal static class IQueryableCloner
    {
        private static ConcurrentDictionary<TwoTypesClass, Type> _c = new ConcurrentDictionary<TwoTypesClass, Type>();

        /// <summary>
        /// Detect if the type implements the expected generic interface
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="expectedInterface">Expected interface</param>
        /// <returns>The expected generic type if found</returns>
        internal static Type GetInterfaceType(Type type, Type expectedInterface)
        {
            var key = new TwoTypesClass
            {
                Type = type,
                OtherType = expectedInterface
            };

            return _c.GetOrAdd(key,
                               k =>
                               {
                                   Type enumerableInterface = null;
                                   if (k.Type.IsGenericType
                                       && k.Type.GetGenericTypeDefinition() == k.OtherType)
                                   {
                                       enumerableInterface = k.Type;
                                   }
                                   else
                                   {
                                       enumerableInterface = k.Type.GetInterfaces()
                                                              .FirstOrDefault(t => t.IsGenericType
                                                                                   && t.GetGenericTypeDefinition() == k.OtherType
                                                                                   && t.GetGenericArguments()
                                                                                       .Length == 1);
                                   }

                                   return enumerableInterface;
                               });
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
                var asq = typeof(Queryable).GetMethod("AsQueryable", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(IEnumerable) }, null);
                return asq.Invoke(null, new[] { obj });
            }

            return obj;
        }
    }
}
