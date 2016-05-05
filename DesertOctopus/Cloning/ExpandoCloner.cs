using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Cloning
{
    internal static class ExpandoCloner
    {
        public static Expression GenerateExpandoObjectExpression(List<ParameterExpression> variables,
                                                                 ParameterExpression source,
                                                                 ParameterExpression clone,
                                                                 ParameterExpression refTrackerParam)
        {
            var enumerableType = typeof(IEnumerable<KeyValuePair<string, object>>);
            var getEnumeratorMethodInfo = IEnumerableMIH.GetEnumerator<string, object>();
            var enumeratorMethod = Expression.Call(Expression.Convert(source, enumerableType), getEnumeratorMethodInfo);
            var dictType = typeof(IDictionary<string, object>);
            var cloneAsDict = Expression.Parameter(dictType, "cloneDict");
            var expressions = new List<Expression>();

            variables.Add(cloneAsDict);

            expressions.Add(Expression.Assign(clone, Expression.New(typeof(ExpandoObject))));
            expressions.Add(Expression.Assign(cloneAsDict, Expression.Convert(clone, dictType)));


            var loopBodyCargo = new EnumerableLoopBodyCargo<string, object>();
            loopBodyCargo.EnumeratorType = typeof(IEnumerator<KeyValuePair<string, object>>);
            loopBodyCargo.KvpType = typeof(KeyValuePair<string, object>);

            expressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop<string, object, IEnumerator<KeyValuePair<string, object>>>(variables,
                                                                                                                          CloneKeyValuePair(clone, refTrackerParam),
                                                                                                                          enumeratorMethod,
                                                                                                                          null,
                                                                                                                          loopBodyCargo));

            return Expression.Block(expressions);
        }


        public static Func<EnumerableLoopBodyCargo<string, object>, Expression> CloneKeyValuePair(ParameterExpression clone,
                                                                                                  ParameterExpression refTrackerParam)
        {
            Func<EnumerableLoopBodyCargo<string, object>, Expression> loopBody = cargo => {
                                                                                              var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Key"));
                                                                                              var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Value"));

                                                                                              var addExpr = Expression.Call(clone,
                                                                                                                            DictionaryMIH.Add<string, object>(),
                                                                                                                            keyExpression,
                                                                                                                            ClassCloner.CallCopyExpression(valueExpression,
                                                                                                                                                           refTrackerParam)
                                                                                                  );
                                                                                              var addNullExpr = Expression.Call(clone,
                                                                                                                                DictionaryMIH.Add<string, object>(),
                                                                                                                                keyExpression,
                                                                                                                                Expression.Constant(null)
                                                                                                  );


                                                                                              return Expression.IfThenElse(Expression.NotEqual(valueExpression, Expression.Constant(null)),
                                                                                                                              addExpr,
                                                                                                                              addNullExpr);
                                                                                            };
            return loopBody;
        }
    }
}