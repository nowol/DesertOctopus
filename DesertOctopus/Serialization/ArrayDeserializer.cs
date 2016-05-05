using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    internal class ArrayDeserializer
    {
        internal static Expression GenerateArrayOfKnownDimension(Type type,
                                                                ParameterExpression inputStream,
                                                                ParameterExpression objTracking)
        {
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var elementType = type.GetElementType();
            var trackType = Expression.Parameter(typeof(byte), "trackType");
            var newInstance = Expression.Parameter(type, "newInstance");
            var i = Expression.Parameter(typeof(int), "i");
            var numberOfDimensions = Expression.Parameter(typeof(int), "numberOfDimensions");
            var lengths = Expression.Parameter(typeof(int[]), "lengths");
            var item = Expression.Parameter(type.GetElementType(), "item");

            variables.Add(trackType);
            variables.Add(newInstance);
            variables.Add(i);
            variables.Add(lengths);
            variables.Add(item);
            variables.Add(numberOfDimensions);

            var rank = type.GetArrayRank();
            List<Expression> notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.AddRange(ReadDimensionalArrayLength(inputStream, lengths, i, numberOfDimensions));
            notTrackedExpressions.AddRange(ReadDimensionalArray(type, elementType, newInstance, variables, inputStream, lengths, objTracking, rank));

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracking,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }

        private static IEnumerable<Expression> ReadDimensionalArray(Type type,
                                                                    Type elementType,
                                                                    ParameterExpression newInstance,
                                                                    List<ParameterExpression> variables,
                                                                    ParameterExpression inputStream,
                                                                    ParameterExpression lengths,
                                                                    ParameterExpression objTracking,
                                                                    int rank)
        {
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "type");
            var indices = Expression.Parameter(typeof(int[]), "indices");
            var tmpValue = Expression.Parameter(elementType, "tmpValue");
            var deserializer = Expression.Parameter(typeof(Func<Stream, List<object>, object>), "deserializer");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");

            variables.Add(deserializer);
            variables.Add(typeName);
            variables.Add(typeExpr);
            variables.Add(indices);
            variables.Add(tmpValue);
            variables.Add(typeHashCode);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(indices, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Property(lengths, "Length"))));
            expressions.Add(Expression.Assign(newInstance, Expression.Convert(Expression.Call(ArrayMIH.CreateInstance(), Expression.Constant(elementType), lengths), type)));
            expressions.Add(Expression.Call(objTracking, ListMIH.ObjectListAdd(), newInstance));

            Expression innerExpression;
            if (elementType.IsPrimitive || elementType.IsValueType || elementType == typeof(string))
            {
                Func<Stream, List<object>, object> primitiveDeserializer = Deserializer.GetTypeDeserializer(elementType);
                innerExpression = Expression.Assign(tmpValue, Expression.Convert(Expression.Invoke(Expression.Constant(primitiveDeserializer), inputStream, objTracking), elementType));
            }
            else
            {
                innerExpression = Deserializer.GetReadClassExpression(inputStream, objTracking, tmpValue, typeExpr, typeName, typeHashCode, deserializer, elementType);
            }

            Func<int, Expression, Expression> readArrayLoop = (loopRank,
                                                              innerExpr) =>
            {
                var loopRankIndex = Expression.Parameter(typeof(int), "loopRankIndex" + loopRank);
                variables.Add(loopRankIndex);

                var loopExpressions = new List<Expression>();

                loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(indices, Expression.Constant(loopRank)), loopRankIndex));
                loopExpressions.Add(innerExpr);
                loopExpressions.Add(Expression.Call(newInstance, ArrayMIH.SetValueRank(), Expression.Convert(tmpValue, typeof(object)), indices));
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
                innerExpression = readArrayLoop(r, innerExpression);
            }

            expressions.Add(innerExpression);

            return expressions;
        }

        private static IEnumerable<Expression> ReadDimensionalArrayLength(ParameterExpression inputStream,
                                                                          Expression lengths,
                                                                          Expression i,
                                                                          Expression numberOfDimensions)
        {
            var loopExpressions = new List<Expression>();
            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));
            expressions.Add(Expression.Assign(numberOfDimensions, Expression.Convert(PrimitiveHelpers.ReadByte(inputStream), typeof(int))));
            expressions.Add(Expression.Assign(lengths, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), numberOfDimensions)));

            loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(lengths, i), PrimitiveHelpers.ReadInt32(inputStream)));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var loopBody = Expression.Block(loopExpressions);
            var breakLabel = Expression.Label("breakLabel");
            var cond = Expression.LessThan(i, numberOfDimensions);
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)),
                                       breakLabel);
            expressions.Add(loop);

            return expressions;
        }
    }
}
