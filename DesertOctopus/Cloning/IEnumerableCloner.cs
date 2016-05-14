using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Cloning
{
    internal static class IEnumerableCloner
    {
        public static Expression GenerateIEnumerableExpression(List<ParameterExpression> variables,
                                                               ParameterExpression source,
                                                               ParameterExpression clone,
                                                               Type sourceType,
                                                               ParameterExpression refTrackerParam)
        {
            Type enumerableInterface;

            if (sourceType.IsGenericType
                && sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                enumerableInterface = sourceType;
            }
            else
            {
                enumerableInterface = sourceType.GetInterfaces()
                                                .FirstOrDefault(t => t.IsGenericType
                                                                     && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                                                     && t.GetGenericArguments()
                                                                         .Length == 1);
            }

            if (enumerableInterface != null)
            {
                var genericArgumentType = enumerableInterface.GetGenericArguments()[0];
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


                //var m = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { genericArgumentType }, Expression.Convert(arrClone, typeof(IEnumerable<>).MakeGenericType(genericArgumentType)));
                //expressions.Add(Expression.Assign(clone, Expression.Convert(m, sourceType)));
                expressions.Add(Expression.Assign(clone, Expression.Convert(arrClone, sourceType)));

                return Expression.Block(expressions);
            }
            else
            {
                throw new NotSupportedException(sourceType.ToString());
            }
        }

        internal static bool IsGenericIEnumerableType(Type sourceType)
        {
            return sourceType.IsGenericType
                   && sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
