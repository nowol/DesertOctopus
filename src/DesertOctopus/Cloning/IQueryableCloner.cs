using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Helper class for IQueryable
    /// </summary>
    internal static class IQueryableCloner
    {
        /// <summary>
        /// Generate an expression that represents an enumerable loop
        /// </summary>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="source">Source object</param>
        /// <param name="clone">Clone object</param>
        /// <param name="sourceType">Type of the source object</param>
        /// <param name="refTrackerParam">Reference tracker</param>
        /// <returns>An expression that represents an enumerable loop</returns>
        public static Expression GenerateEnumeratingExpression(List<ParameterExpression> variables,
                                                               ParameterExpression source,
                                                               ParameterExpression clone,
                                                               Type sourceType,
                                                               ParameterExpression refTrackerParam)
        {
            Type enumerableInterface = GetInterfaceType(sourceType, typeof(IEnumerable<>));

            if (enumerableInterface == null)
            {
                throw new NotSupportedException(sourceType.ToString());
            }

            Type queryableInterface = GetInterfaceType(sourceType, typeof(IQueryable<>));

            var genericArgumentType = enumerableInterface.GetGenericArguments()[0];
            var arrayType = genericArgumentType.MakeArrayType();
            var arrSource = Expression.Parameter(arrayType, "arrSource");
            var arrClone = Expression.Parameter(arrayType, "arrClone");
            var cloner = ObjectCloner.CloneImpl(arrayType);
            var toArrayMethod = typeof(System.Linq.Enumerable).GetMethod("ToArray").MakeGenericMethod(genericArgumentType);

            variables.Add(arrSource);
            variables.Add(arrClone);

            var expressions = new List<Expression>();

            expressions.Add(Expression.Assign(arrSource, Expression.Call(toArrayMethod, source)));

            var c = Expression.Invoke(Expression.Constant(cloner), Expression.Convert(arrSource, typeof(object)), refTrackerParam);
            expressions.Add(Expression.Assign(arrClone, Expression.Convert(c, arrayType)));

            if (queryableInterface != null)
            {
                var m = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { genericArgumentType }, Expression.Convert(arrClone, typeof(IEnumerable<>).MakeGenericType(genericArgumentType)));
                expressions.Add(Expression.Assign(clone, Expression.Convert(m, sourceType)));
                expressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));
            }
            else
            {
                expressions.Add(Expression.Assign(clone, Expression.Convert(arrClone, sourceType)));
                expressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));
            }

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(expressions));
        }

        /// <summary>
        /// Detect if the type implements the expected generic interface
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <param name="expectedInterface">Expected interface</param>
        /// <returns>The expected generic type if found</returns>
        internal static Type GetInterfaceType(Type type, Type expectedInterface)
        {
            Type enumerableInterface = null;
            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == expectedInterface)
            {
                enumerableInterface = type;
            }
            else
            {
                enumerableInterface = type.GetInterfaces()
                                                .FirstOrDefault(t => t.IsGenericType
                                                                     && t.GetGenericTypeDefinition() == expectedInterface
                                                                     && t.GetGenericArguments()
                                                                         .Length == 1);
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
                var asq = typeof(Queryable).GetMethod("AsQueryable", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(IEnumerable) }, null);
                return asq.Invoke(null, new[] { obj });
            }

            return obj;
        }
    }
}
