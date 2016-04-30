using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class EnumerableLoopHelper
    {

        public static Func<EnumerableLoopBodyCargo<string, object>, Expression> GetStringToSomethingWriter(ParameterExpression outputStream, ParameterExpression objTracking)
        {
            Func<EnumerableLoopBodyCargo<string, object>, Expression> loopBody = cargo => {
                var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Key"));
                var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Value"));

                return Expression.Block(PrimitiveHelpers.WriteString(outputStream, keyExpression),
                                                       Serializer.GetWriteClassTypeExpression(outputStream, objTracking, valueExpression, cargo.ItemAsObj, cargo.TypeExpr, cargo.Serializer, typeof(object)));
            };
            return loopBody;
        }

        public static Expression GenerateEnumeratorLoop<TKey, TValue, TEnumeratorType>
                                                                    (Type type,
                                                                      List<ParameterExpression> variables,
                                                                      ParameterExpression outputStream,
                                                                      ParameterExpression objToSerialize,
                                                                      ParameterExpression objTracking,
                                                                      Func<EnumerableLoopBodyCargo<TKey, TValue>, Expression> loopBody,
                                                                      Expression countExpression,
                                                                      Expression getEnumeratorMethod,
                                                                      EnumerableLoopBodyCargo<TKey, TValue> loopBodyCargo)
        {
            var breakLabel = Expression.Label("breakLabel");
            var enumeratorVar = Expression.Parameter(typeof(TEnumeratorType), "enumeratorVar");

            var serializer = Expression.Parameter(typeof(Action<Stream, object, SerializerObjectTracker>), "serializer");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");

            variables.Add(serializer);
            variables.Add(typeExpr);
            variables.Add(itemAsObj);
            variables.Add(enumeratorVar);

            loopBodyCargo.Enumerator = enumeratorVar;
            loopBodyCargo.TypeExpr = typeExpr;
            loopBodyCargo.ItemAsObj = itemAsObj;
            loopBodyCargo.Serializer = serializer;

            Expression finallyExpr;
            if (typeof(IDisposable).IsAssignableFrom(typeof(TEnumeratorType)))
            {
                finallyExpr = Expression.IfThen(Expression.NotEqual(enumeratorVar, Expression.Constant(null)),
                                                Expression.Call(enumeratorVar, IDisposableMIH.Dispose()));
            }
            else
            {
                finallyExpr = Expression.Empty();
            }

            return Expression.TryFinally(Expression.Block(Expression.Assign(enumeratorVar, getEnumeratorMethod),
                                                          PrimitiveHelpers.WriteInt32(outputStream, countExpression),
                                                          Expression.Loop(Expression.IfThenElse(Expression.IsTrue(Expression.Call(enumeratorVar, IEnumeratorMIH.MoveNext())),
                                                                                                loopBody(loopBodyCargo),
                                                                                                Expression.Break(breakLabel)),
                                                                          breakLabel)),
                                         finallyExpr);
        }
    }

    internal class EnumerableLoopBodyCargo<TKey, TValue>
    {
        public ParameterExpression ItemAsObj { get; set; }
        public ParameterExpression Enumerator { get; set; }
        public Type EnumeratorType { get; set; }
        public ParameterExpression TypeExpr { get; set; }
        public ParameterExpression Serializer { get; set; }
        public Type KvpType { get; set; }
    }
}
