using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    internal static class IQueryableCloner
    {
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
                expressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.Track(), source, clone));
            }
            else
            {
                expressions.Add(Expression.Assign(clone, Expression.Convert(arrClone, sourceType)));
                expressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.Track(), source, clone));
            }

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(expressions));
        }

        internal static Type GetInterfaceType(Type sourceType, Type expectedInterface)
        {
            Type enumerableInterface = null;
            if (sourceType.IsGenericType
                && sourceType.GetGenericTypeDefinition() == expectedInterface)
            {
                enumerableInterface = sourceType;
            }
            else
            {
                enumerableInterface = sourceType.GetInterfaces()
                                                .FirstOrDefault(t => t.IsGenericType
                                                                     && t.GetGenericTypeDefinition() == expectedInterface
                                                                     && t.GetGenericArguments()
                                                                         .Length == 1);
            }
            return enumerableInterface;
        }

        public static Expression GenerateIQueryableExpression(List<ParameterExpression> variables,
                                                              ParameterExpression source,
                                                              ParameterExpression clone,
                                                              Type sourceType,
                                                              ParameterExpression refTrackerParam)
        {
            Type queryableInterface;

            if (IsGenericIQueryableType(sourceType))
            {
                queryableInterface = sourceType;
            }
            else
            {
                queryableInterface = sourceType.GetInterfaces()
                                               .FirstOrDefault(t => t.IsGenericType
                                                                    && t.GetGenericTypeDefinition() == typeof(IQueryable<>)
                                                                    && t.GetGenericArguments()
                                                                        .Length == 1);
            }

            if (queryableInterface != null)
            {
                var genericArgumentType = queryableInterface.GetGenericArguments()[0];
                var arrayType = genericArgumentType.MakeArrayType();
                var arrSource = Expression.Parameter(arrayType, "arrSource");
                var arrClone = Expression.Parameter(arrayType, "arrClone");
                var cloner = ObjectCloner.CloneImpl(arrayType);

                variables.Add(arrSource);
                variables.Add(arrClone);

                var expressions = new List<Expression>();
                var toArrayMethod = typeof(System.Linq.Enumerable).GetMethod("ToArray").MakeGenericMethod(genericArgumentType);

                expressions.Add(Expression.Assign(arrSource, Expression.Call(toArrayMethod, source)));

                //object objToClone = ObjectCleaner.PrepareObjectForSerialization(obj);


                var c = Expression.Invoke(Expression.Constant(cloner), Expression.Convert(arrSource, typeof(object)), refTrackerParam);
                expressions.Add(Expression.Assign(arrClone, Expression.Convert(c, arrayType)));


                //IEnumerable<string> l = new List<string>();
                //l.ToArray();


                var m = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { genericArgumentType }, Expression.Convert(arrClone, typeof(IEnumerable<>).MakeGenericType(genericArgumentType)));
                expressions.Add(Expression.Assign(clone, Expression.Convert(m, sourceType)));

                //System.Linq.Enumerable.ToArray()


                //IEnumerable<IGrouping<string, string>> ll = l.GroupBy(x => x);


                return Expression.Block(expressions);
            }
            else
            {
                throw new NotSupportedException(sourceType.ToString());
            }
        }

        internal static bool IsGenericIQueryableType(Type sourceType)
        {
            return sourceType.IsGenericType
                   && sourceType.GetGenericTypeDefinition() == typeof(IQueryable<>);
        }


        //public void a()
        //{
        //    var enumerableValue = objToPrepare as IEnumerable;
        //    if (enumerableValue != null)
        //    {
        //        var objectType = objToPrepare.GetType();
        //        if (objectType.IsArray
        //            || typeof(IList).IsAssignableFrom(objectType)
        //            || typeof(ICollection).IsAssignableFrom(objectType))
        //        {
        //            return objToPrepare;
        //        }

        //        if (enumerableValue.GetType().DeclaringType == typeof(System.Linq.Enumerable)
        //            || (!String.IsNullOrWhiteSpace(enumerableValue.GetType().Namespace) && enumerableValue.GetType().Namespace.StartsWith("System.Linq")))
        //        {
        //            Type itemType = typeof(object);

        //            var enumerableInterface = objectType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        //            if (enumerableInterface != null)
        //            {
        //                itemType = enumerableInterface.GetGenericArguments()[0];
        //            }

        //            var converter = SerializerMIH.ConvertEnumerableToArray(itemType);
        //            return converter.Invoke(null,
        //                                    new object[]
        //                                    {
        //                                        enumerableValue
        //                                    });
        //        }
        //    }


        //    return objToPrepare;
        //}
    }
}
