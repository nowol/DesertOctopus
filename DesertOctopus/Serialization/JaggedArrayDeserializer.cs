using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Serialization.Helpers;

namespace DesertOctopus.Serialization
{
    internal static class JaggedArrayDeserializer
    {
        public static Expression GenerateJaggedArray(Type type,
                                                     ParameterExpression inputStream,
                                                     ParameterExpression objTracking)
        {
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var elementType = type.GetElementType();
            var trackType = Expression.Parameter(typeof(byte), "trackType");
            var newInstance = Expression.Parameter(type, "newInstance");
            var numberOfDimensions = Expression.Parameter(typeof(int), "numberOfDimensions");
            var lengths = Expression.Parameter(typeof(int[]), "lengths");
            var item = Expression.Parameter(type.GetElementType(), "item");

            variables.Add(trackType);
            variables.Add(newInstance);
            variables.Add(lengths);
            variables.Add(item);
            variables.Add(numberOfDimensions);

            List<Expression> notTrackedExpressions = new List<Expression>();

            notTrackedExpressions.AddRange(ReadJaggedArray(type, elementType, newInstance, variables, inputStream, objTracking));

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracking,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }

        private static IEnumerable<Expression> ReadJaggedArray(Type type,
                                                               Type elementType,
                                                               ParameterExpression newInstance,
                                                               List<ParameterExpression> variables,
                                                               ParameterExpression inputStream,
                                                               ParameterExpression objTracking)
        {
            var i = Expression.Parameter(typeof(int), "i");
            var length = Expression.Parameter(typeof(int), "length");
            var item = Expression.Parameter(type.GetElementType(), "item");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "type");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");
            var tmpValue = Expression.Parameter(elementType, "tmpValue");
            var deserializer = Expression.Parameter(typeof(Func<Stream, List<object>, object>), "deserializer");

            variables.Add(length);
            variables.Add(item);
            variables.Add(typeName);
            variables.Add(typeExpr);
            variables.Add(typeHashCode);
            variables.Add(i);
            variables.Add(tmpValue);
            variables.Add(deserializer);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(length, PrimitiveHelpers.ReadInt32(inputStream)));
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));
            expressions.Add(Expression.Assign(newInstance, Expression.Convert(Expression.New(type.GetConstructor(new []{ typeof(int)}), length), type)));
            expressions.Add(Expression.Call(objTracking, typeof(List<object>).GetMethod("Add"), newInstance));

            var loopExpressions = new List<Expression>();
            loopExpressions.Add(Deserializer.GetReadClassExpression(inputStream, objTracking, tmpValue, typeExpr, typeName, typeHashCode, deserializer, elementType));
            loopExpressions.Add(Expression.Call(newInstance, typeof(Array).GetMethod("SetValue", new[] { typeof(object), typeof(int) }), Expression.Convert(tmpValue, typeof(object)), i));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(loopExpressions);
            var breakLabel = Expression.Label("breakLabel");
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)
                                                            ),
                                        breakLabel);
            expressions.Add(loop);
            return expressions;
        }
    }
}
