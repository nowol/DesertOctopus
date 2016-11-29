using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using DesertOctopus.Polyfills;
using DesertOctopus.Utilities;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Helper class that handles ExpandoObject
    /// </summary>
    internal static class ExpandoCloner
    {
        /// <summary>
        /// Generate an expression tree that clones ExpandoObject
        /// </summary>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="source">Source object</param>
        /// <param name="clone">Clone object</param>
        /// <param name="refTrackerParam">Reference tracker</param>
        /// <returns>An expression tree that clones ExpandoObject</returns>
        public static Expression GenerateExpandoObjectExpression(List<ParameterExpression> variables,
                                                                 ParameterExpression source,
                                                                 ParameterExpression clone,
                                                                 ParameterExpression refTrackerParam)
        {
            var enumerableType = typeof(IEnumerable<KeyValuePair<string, object>>);
            var getEnumeratorMethodInfo = IEnumerableMih.GetEnumerator<string, object>();
            var enumeratorMethod = Expression.Call(Expression.Convert(source, enumerableType), getEnumeratorMethodInfo);
            var dictType = typeof(IDictionary<string, object>);
            var cloneAsDict = Expression.Parameter(dictType, "cloneDict");
            var expressions = new List<Expression>();

            variables.Add(cloneAsDict);

            expressions.Add(Expression.Assign(clone, Expression.New(typeof(ExpandoObject))));
            expressions.Add(Expression.Assign(cloneAsDict, Expression.Convert(clone, dictType)));
            expressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));

            var loopBodyCargo = new EnumerableLoopBodyCargo();
            loopBodyCargo.EnumeratorType = typeof(IEnumerator<KeyValuePair<string, object>>);
            loopBodyCargo.KvpType = typeof(KeyValuePair<string, object>);

            expressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop(variables,
                                                                        CloneKeyValuePair(clone, refTrackerParam),
                                                                        enumeratorMethod,
                                                                        null,
                                                                        loopBodyCargo));

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         typeof(ExpandoObject),
                                                                         refTrackerParam,
                                                                         Expression.Block(expressions));
        }

        private static Func<EnumerableLoopBodyCargo, Expression> CloneKeyValuePair(ParameterExpression clone,
                                                                                                   ParameterExpression refTrackerParam)
        {
            Func<EnumerableLoopBodyCargo, Expression> loopBody = cargo =>
            {
                var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Key"));
                var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Value"));

                var addExpr = Expression.Call(clone,
                                              DictionaryMih.Add<string, object>(),
                                              keyExpression,
                                              ClassCloner.CallCopyExpression(valueExpression,
                                                                             refTrackerParam,
                                                                             Expression.Call(valueExpression, ObjectMih.GetTypeMethod())));
                var addNullExpr = Expression.Call(clone,
                                                DictionaryMih.Add<string, object>(),
                                                keyExpression,
                                                Expression.Constant(null));

                return Expression.IfThenElse(Expression.NotEqual(valueExpression, Expression.Constant(null)),
                                                addExpr,
                                                addNullExpr);
            };
            return loopBody;
        }
    }
}