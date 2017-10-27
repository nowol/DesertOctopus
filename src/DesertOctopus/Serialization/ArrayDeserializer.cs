using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle array deserialization
    /// </summary>
    internal static class ArrayDeserializer
    {
        /// <summary>
        /// Generate an expression tree to handle array deserialization
        /// </summary>
        /// <param name="type">Type of the array</param>
        /// <param name="inputStream">Stream that is read from</param>
        /// <param name="objTracker">Reference tracker</param>
        /// <returns>An expression tree to handle array deserialization</returns>
        internal static Expression GenerateArrayOfKnownDimension(Type type,
                                                                 ParameterExpression inputStream,
                                                                 ParameterExpression objTracker)
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
            notTrackedExpressions.AddRange(ReadDimensionalArrayLength(inputStream, objTracker, lengths, i, numberOfDimensions));
            notTrackedExpressions.AddRange(ReadDimensionalArray(type, elementType, newInstance, variables, inputStream, lengths, objTracker, rank));

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracker,
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
                                                                    ParameterExpression objTracker,
                                                                    int rank)
        {
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "type");
            var indices = Expression.Parameter(typeof(int[]), "indices");
            var tmpValue = Expression.Parameter(elementType, "tmpValue");
            var deserializer = Expression.Parameter(typeof(Func<Stream, DeserializerObjectTracker, object>), "deserializer");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");

            variables.Add(deserializer);
            variables.Add(typeName);
            variables.Add(typeExpr);
            variables.Add(tmpValue);
            variables.Add(typeHashCode);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(newInstance, Expression.Convert(Expression.Call(ArrayMih.CreateInstance(), Expression.Constant(elementType), lengths), type)));
            if (type.IsClass)
            {
                expressions.Add(Expression.Call(objTracker, DeserializerObjectTrackerMih.TrackedObject(), newInstance));
            }

            if (rank > 1)
            {
                variables.Add(indices);
                expressions.Add(Expression.Assign(indices, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Property(lengths, "Length"))));
            }

            Expression innerExpression;

            if (elementType == typeof(string))
            {
                innerExpression = Expression.Assign(tmpValue, Deserializer.GenerateStringExpression(inputStream, objTracker));
            }
            else if (elementType.GetTypeInfo().IsPrimitive || elementType.GetTypeInfo().IsValueType)
            {
                var primitiveReader = Deserializer.GetPrimitiveReader(elementType);
                if (primitiveReader == null)
                {
                    var primitiveDeserializer = Deserializer.GetTypeDeserializer(elementType);
                    innerExpression = Expression.Assign(tmpValue, Expression.Convert(Expression.Invoke(Expression.Constant(primitiveDeserializer), inputStream, objTracker), elementType));
                }
                else
                {
                    if (elementType == typeof(byte)
                        || elementType == typeof(sbyte)
                        || elementType == typeof(byte?)
                        || elementType == typeof(sbyte?)
                        || Deserializer.IsEnumOrNullableEnum(elementType))
                    {
                        innerExpression = Expression.Assign(tmpValue, Expression.Convert(primitiveReader(inputStream, objTracker), elementType));
                    }
                    else
                    {
                        innerExpression = Expression.Assign(tmpValue, primitiveReader(inputStream, objTracker));
                    }
                }
            }
            else
            {
                innerExpression = Deserializer.GetReadClassExpression(inputStream, objTracker, tmpValue, typeExpr, typeName, typeHashCode, deserializer, elementType);
            }

            Func<int, Expression, Expression> readArrayLoop = (loopRank,
                                                              innerExpr) =>
            {
                var loopRankIndex = Expression.Parameter(typeof(int), "loopRankIndex" + loopRank);
                variables.Add(loopRankIndex);

                var loopExpressions = new List<Expression>();



                if (rank == 1)
                {
                    loopExpressions.Add(innerExpr);
                    loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(newInstance, loopRankIndex), tmpValue));
                }
                else
                {
                    loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(indices, Expression.Constant(loopRank)), loopRankIndex));
                    loopExpressions.Add(innerExpr);
                    loopExpressions.Add(Expression.Call(newInstance, ArrayMih.SetValueRank(), Expression.Convert(tmpValue, typeof(object)), indices));
                }

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
                                                                          ParameterExpression objTracker,
                                                                          Expression lengths,
                                                                          Expression i,
                                                                          Expression numberOfDimensions)
        {
            var loopExpressions = new List<Expression>();
            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));
            expressions.Add(Expression.Assign(numberOfDimensions, Expression.Convert(PrimitiveHelpers.ReadByte(inputStream, objTracker), typeof(int))));
            expressions.Add(Expression.Assign(lengths, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), numberOfDimensions)));

            loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(lengths, i), PrimitiveHelpers.ReadInt32(inputStream, objTracker)));
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
