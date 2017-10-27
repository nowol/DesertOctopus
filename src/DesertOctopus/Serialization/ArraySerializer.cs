using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle array serialization
    /// </summary>
    internal static class ArraySerializer
    {
        /// <summary>
        /// Generate an expression tree to handle array serialization
        /// </summary>
        /// <param name="type">Type of the array</param>
        /// <param name="elementType">Type of the elements contained inside the array</param>
        /// <param name="outputStream">Stream that is written to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to handle array serialization</returns>
        internal static Expression GenerateArrayOfKnownDimension(Type type,
                                                                 Type elementType,
                                                                 ParameterExpression outputStream,
                                                                 ParameterExpression objToSerialize,
                                                                 ParameterExpression objTracking)
        {
            List<Expression> notTrackedExpressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var i = Expression.Parameter(typeof(int), "i");
            var lengths = Expression.Parameter(typeof(int[]), "lengths");
            var trackedObjectPosition = Expression.Parameter(typeof(int?), "trackedObjectPosition");
            var arr = Expression.Parameter(type, "arr");
            var rank = type.GetArrayRank();

            variables.Add(trackedObjectPosition);
            variables.Add(i);
            variables.Add(lengths);
            variables.Add(arr);

            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));
            notTrackedExpressions.Add(Expression.Assign(arr, Expression.Convert(objToSerialize, type)));
            notTrackedExpressions.Add(Expression.Assign(lengths, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Constant(rank))));
            notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            notTrackedExpressions.Add(Expression.IfThen(Expression.GreaterThanOrEqual(Expression.Constant(rank), Expression.Constant(255)),
                                                        Expression.Throw(Expression.New(NotSupportedExceptionMih.ConstructorString(), Expression.Constant("Array with more than 255 dimensions are not supported")))));
            notTrackedExpressions.AddRange(WriteDimensionalArrayLength(outputStream, objTracking, i, arr, lengths, rank));
            notTrackedExpressions.AddRange(WriteDimensionalArray(elementType, variables, outputStream, arr, rank, lengths, objTracking));

            return Serializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                       outputStream,
                                                                       objToSerialize,
                                                                       objTracking,
                                                                       notTrackedExpressions,
                                                                       variables);
        }

        private static IEnumerable<Expression> WriteDimensionalArray(Type elementType,
                                                                     List<ParameterExpression> variables,
                                                                     ParameterExpression outputStream,
                                                                     ParameterExpression arr,
                                                                     int rank,
                                                                     ParameterExpression lengths,
                                                                     ParameterExpression objTracking)
        {
            var item = Expression.Parameter(elementType, "item");
            var serializer = Expression.Parameter(typeof(Action<Stream, object, SerializerObjectTracker>), "serializer");
            var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var indices = Expression.Parameter(typeof(int[]), "indices");
            variables.Add(typeExpr);
            variables.Add(itemAsObj);
            variables.Add(serializer);
            variables.Add(item);

            var expressions = new List<Expression>();
            if (rank != 1)
            {
                variables.Add(indices);
                expressions.Add(Expression.Assign(indices, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Constant(rank))));
            }

            Expression innerExpression;

            if (elementType == typeof(string))
            {
                innerExpression = Serializer.GenerateStringExpression(outputStream, item, objTracking);
            }
            else if (elementType.GetTypeInfo().IsPrimitive || elementType.GetTypeInfo().IsValueType)
            {
                var primitiveWriter = Serializer.GetPrimitiveWriter(elementType);

                if (primitiveWriter == null)
                {
                    var primitiveSerializer = Serializer.GetTypeSerializer(elementType);
                    innerExpression = Expression.Invoke(Expression.Constant(primitiveSerializer), outputStream, item, objTracking);
                }
                else
                {
                    innerExpression = primitiveWriter(outputStream, item, objTracking);
                }
            }
            else
            {
                innerExpression = Serializer.GetWriteClassTypeExpression(outputStream, objTracking, item, itemAsObj, typeExpr, serializer, elementType);
            }

            Func<int, Expression, Expression> makeArrayLoop = (loopRank,
                                                               innerExpr) =>
            {
                var loopRankIndex = Expression.Parameter(typeof(int), "loopRankIndex" + loopRank);
                variables.Add(loopRankIndex);

                var loopExpressions = new List<Expression>();

                if (rank == 1)
                {
                    loopExpressions.Add(Expression.Assign(item, Expression.ArrayAccess(arr, loopRankIndex)));
                }
                else
                {
                    loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(indices, Expression.Constant(loopRank)), loopRankIndex));
                    loopExpressions.Add(Expression.Assign(item, Expression.Convert(Expression.Call(arr, ArrayMih.GetValueRank(), indices), elementType)));
                }

                loopExpressions.Add(innerExpr);
                loopExpressions.Add(Expression.Assign(loopRankIndex, Expression.Add(loopRankIndex, Expression.Constant(1))));

                var cond = Expression.LessThan(loopRankIndex, Expression.ArrayIndex(lengths, Expression.Constant(loopRank)));
                var loopBody = Expression.Block(loopExpressions);

                var breakLabel = Expression.Label("breakLabel" + loopRank);
                var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                                 loopBody,
                                                                 Expression.Break(breakLabel)),
                                            breakLabel);
                return Expression.Block(Expression.Assign(loopRankIndex, Expression.Constant(0)),
                                        loop);
            };

            for (int r = rank - 1; r >= 0; r--)
            {
                innerExpression = makeArrayLoop(r, innerExpression);
            }

            expressions.Add(innerExpression);

            return expressions;
        }

        private static IEnumerable<Expression> WriteDimensionalArrayLength(ParameterExpression outputStream,
                                                                           Expression objTracking,
                                                                           Expression i,
                                                                           ParameterExpression arr,
                                                                           ParameterExpression lengths,
                                                                           int rank)
        {
            var expressions = new List<Expression>();
            expressions.Add(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)rank), objTracking));

            var loopExpressions = new List<Expression>();
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));

            var length = Expression.Call(arr, ArrayMih.GetLength(), i);
            loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(lengths, i), length));
            loopExpressions.Add(PrimitiveHelpers.WriteInt32(outputStream, Expression.ArrayIndex(lengths, i), objTracking));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var loopBody = Expression.Block(loopExpressions);
            var breakLabel = Expression.Label("breakLabel");
            var cond = Expression.LessThan(i, Expression.Constant(rank));
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                                loopBody,
                                                                Expression.Break(breakLabel)),
                                        breakLabel);
            expressions.Add(loop);

            return expressions;
        }
    }
}
