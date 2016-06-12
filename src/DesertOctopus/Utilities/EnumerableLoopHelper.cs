using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for enumerable loops
    /// </summary>
    internal static class EnumerableLoopHelper
    {
        /// <summary>
        /// Gets the standard string/object expression body
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>The standard string/object expression body</returns>
        public static Func<EnumerableLoopBodyCargo, Expression> GetStringToSomethingWriter(ParameterExpression outputStream, ParameterExpression objTracking)
        {
            Func<EnumerableLoopBodyCargo, Expression> loopBody = cargo =>
            {
                var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Key"));
                var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Value"));

                return Expression.Block(Serializer.GenerateStringExpression(outputStream, keyExpression, objTracking),
                                        Serializer.GetWriteClassTypeExpression(outputStream, objTracking, valueExpression, cargo.ItemAsObj, cargo.TypeExpr, cargo.Serializer, typeof(object)));
            };
            return loopBody;
        }

        /// <summary>
        /// Generates an expression tree that represents an enumerator loop
        /// </summary>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="loopBody">Expression that represents the loop body</param>
        /// <param name="getEnumeratorMethod">Expression that returns the enumerator</param>
        /// <param name="preLoopActions">Expression that represents the action to execute before the loop</param>
        /// <param name="loopBodyCargo">Helper class to hold loop body information</param>
        /// <returns>An expression tree that represents an enumerator loop</returns>
        public static Expression GenerateEnumeratorLoop(List<ParameterExpression> variables,
                                                        Func<EnumerableLoopBodyCargo, Expression> loopBody,
                                                        Expression getEnumeratorMethod,
                                                        IEnumerable<Expression> preLoopActions,
                                                        EnumerableLoopBodyCargo loopBodyCargo)
        {
            var breakLabel = Expression.Label("breakLabel");
            var enumeratorVar = Expression.Parameter(loopBodyCargo.EnumeratorType, "enumeratorVar");

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

            Expression finallyExpr = Expression.IfThen(Expression.TypeIs(enumeratorVar, typeof(IDisposable)),
                                                       Expression.Call(Expression.Convert(enumeratorVar, typeof(IDisposable)), IDisposableMIH.Dispose()));

            var expressions = new List<Expression>();

            expressions.Add(Expression.Assign(enumeratorVar, getEnumeratorMethod));
            if (preLoopActions != null)
            {
                expressions.AddRange(preLoopActions);
            }

            expressions.Add(Expression.Loop(Expression.IfThenElse(Expression.IsTrue(Expression.Call(enumeratorVar, IEnumeratorMIH.MoveNext())),
                                                                                                loopBody(loopBodyCargo),
                                                                                                Expression.Break(breakLabel)),
                                            breakLabel));
            return Expression.TryFinally(Expression.Block(expressions),
                                         finallyExpr);
        }
    }
}
