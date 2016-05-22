﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Cloning;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class that help cleaning objects to serialize/clone
    /// </summary>
    internal class ObjectCleaner
    {
        /// <summary>
        /// Convert an IEnumerable to an array
        /// </summary>
        /// <param name="objToPrepare">Object to convert</param>
        /// <returns>The converted object</returns>
        internal static object PrepareObjectForSerialization(object objToPrepare)
        {
            var enumerableValue = objToPrepare as IEnumerable;
            if (enumerableValue != null)
            {
                var objectType = objToPrepare.GetType();
                if (objectType.IsArray
                    || typeof(IList).IsAssignableFrom(objectType)
                    || typeof(ICollection).IsAssignableFrom(objectType))
                {
                    return objToPrepare;
                }

                if (IsEnumeratingType(enumerableValue))
                {
                    Type itemType = typeof(object);

                    var enumerableInterface = objectType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    if (enumerableInterface != null)
                    {
                        itemType = enumerableInterface.GetGenericArguments()[0];
                    }

                    var converter = SerializerMIH.ConvertEnumerableToArray(itemType);
                    return converter.Invoke(null,
                                            new object[]
                                            {
                                                enumerableValue
                                            });
                }
            }

            return objToPrepare;
        }

        /// <summary>
        /// Gets the type of the enumerating object
        /// </summary>
        /// <param name="enumerableValue">Object to analyze</param>
        /// <returns>The type of the enumerating object</returns>
        internal static bool IsEnumeratingType(IEnumerable enumerableValue)
        {
            if (enumerableValue == null)
            {
                return false;
            }

            var type = enumerableValue.GetType();
            return IsEnumeratingType(type);
        }

        /// <summary>
        /// Detect if the type is enumerating
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>True if the type is defined in System.Linq.Enumerable</returns>
        internal static bool IsEnumeratingType(Type type)
        {
            return type.DeclaringType == typeof(System.Linq.Enumerable)
                   || (!String.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.StartsWith("System.Linq"));
        }

        /// <summary>
        /// Convert an IEnumerable to an array
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <param name="expectedType">Expected type</param>
        /// <returns>The converted object</returns>
        internal static object ConvertObjectToExpectedType(object obj, Type expectedType)
        {
            if (obj == null)
            {
                return obj;
            }

            var objectType = obj.GetType();
            if (objectType == expectedType
                || objectType.IsSubclassOf(expectedType))
            {
                return obj;
            }

            if (IQueryableCloner.IsGenericIQueryableType(expectedType))
            {
                var m = QueryableMIH.AsQueryable(expectedType.GetGenericArguments()[0]);
                return m.Invoke(null, new[] { obj });
            }

            if (typeof(IQueryable).IsAssignableFrom(expectedType))
            {
                return IQueryableCloner.ConvertToNonGenericQueryable(obj);
            }

            return obj;
        }
    }
}