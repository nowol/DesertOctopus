using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    internal static class ISerializableDeserializer
    {
        public static Expression GenerateISerializableExpression(Type type,
                                                                 List<ParameterExpression> variables,
                                                                 ParameterExpression inputStream,
                                                                 ParameterExpression objTracking)
        {
            var length = Expression.Parameter(typeof(int), "length");
            var i = Expression.Parameter(typeof(int), "i");
            var key = Expression.Parameter(typeof(string), "key");
            var value = Expression.Parameter(typeof(object), "value");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "type");
            var deserializer = Expression.Parameter(typeof(Func<Stream, List<object>, object>), "deserializer");
            var newInstance = Expression.Parameter(type, "newInstance");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");
            var fc = Expression.Parameter(typeof(FormatterConverter), "fc");
            var context = Expression.Parameter(typeof(StreamingContext), "context");
            var si = Expression.Parameter(typeof(SerializationInfo), "si");

            variables.Add(fc);
            variables.Add(context);
            variables.Add(si);
            variables.Add(key);
            variables.Add(value);
            variables.Add(length);
            variables.Add(i);
            variables.Add(deserializer);
            variables.Add(typeName);
            variables.Add(typeExpr);
            variables.Add(newInstance);
            variables.Add(typeHashCode);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(fc, Expression.New(typeof(FormatterConverter))));
            expressions.Add(Expression.Assign(context, Expression.New(StreamingContextMIH.Constructor(), Expression.Constant(StreamingContextStates.All))));
            expressions.Add(Expression.Assign(si, Expression.New(SerializationInfoMIH.Constructor(), Expression.Constant(type), fc)));
            expressions.Add(Expression.Assign(length, PrimitiveHelpers.ReadInt32(inputStream)));
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));

            var loopExpressions = new List<Expression>();
            loopExpressions.Add(Expression.Assign(key, Deserializer.GenerateStringExpression(inputStream, objTracking)));
            loopExpressions.Add(Deserializer.GetReadClassExpression(inputStream, objTracking, value, typeExpr, typeName, typeHashCode, deserializer, typeof(object)));
            loopExpressions.Add(Expression.Call(si, SerializationInfoMIH.AddValue(), key, value));
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
            expressions.Add(Expression.Assign(newInstance, Expression.New(ISerializableSerializer.GetSerializationConstructor(type), si, context)));
            expressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializedAttributeExpression(type, newInstance, context));
            expressions.Add(SerializationCallbacksHelper.GenerateCallIDeserializationExpression(type, newInstance));
            expressions.Add(newInstance);

            return Expression.Block(expressions);

        }
    }
}
