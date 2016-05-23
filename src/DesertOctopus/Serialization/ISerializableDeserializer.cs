using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle ISerializable deserialization
    /// </summary>
    internal static class ISerializableDeserializer
    {
        /// <summary>
        /// Generates an expression tree to handle ISerializable deserialization
        /// </summary>
        /// <param name="type">Type to deserialize</param>
        /// <param name="variables">Global variables for expression tree</param>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to handle ISerializable deserialization</returns>
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
            var trackType = Expression.Parameter(typeof(byte), "trackType");

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
            variables.Add(trackType);

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
                                                             Expression.Break(breakLabel)),
                                        breakLabel);

            var notTrackedExpressions = new List<Expression>();

            notTrackedExpressions.Add(Expression.Assign(fc, Expression.New(typeof(FormatterConverter))));
            notTrackedExpressions.Add(Expression.Assign(context, Expression.New(StreamingContextMIH.Constructor(), Expression.Constant(StreamingContextStates.All))));
            notTrackedExpressions.Add(Expression.Assign(si, Expression.New(SerializationInfoMIH.Constructor(), Expression.Constant(type), fc)));
            notTrackedExpressions.Add(Expression.Assign(length, PrimitiveHelpers.ReadInt32(inputStream)));
            notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            notTrackedExpressions.Add(loop);
            notTrackedExpressions.Add(Expression.Assign(newInstance, Expression.New(ISerializableSerializer.GetSerializationConstructor(type), si, context)));
            notTrackedExpressions.Add(Expression.Call(objTracking, ListMIH.ObjectListAdd(), newInstance));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializedAttributeExpression(type, newInstance, context));
            notTrackedExpressions.Add(SerializationCallbacksHelper.GenerateCallIDeserializationExpression(type, newInstance));
            notTrackedExpressions.Add(newInstance);

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracking,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }
    }
}
