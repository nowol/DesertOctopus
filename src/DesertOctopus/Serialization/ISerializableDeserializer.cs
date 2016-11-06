using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
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
        /// <param name="objTracker">Reference tracker</param>
        /// <returns>An expression tree to handle ISerializable deserialization</returns>
        public static Expression GenerateISerializableExpression(Type type,
                                                                 List<ParameterExpression> variables,
                                                                 ParameterExpression inputStream,
                                                                 ParameterExpression objTracker)
        {
            var newInstance = Expression.Parameter(type, "newInstance");
            var trackType = Expression.Parameter(typeof(byte), "trackType");
            variables.Add(newInstance);
            variables.Add(trackType);

            var dictionaryType = DictionaryHelper.GetDictionaryType(type, throwIfNotADictionary: false);
            var notTrackedExpressions = new List<Expression>();

            if (dictionaryType == null)
            {
                notTrackedExpressions.Add(PrimitiveHelpers.ReadByte(inputStream, objTracker));
                notTrackedExpressions.Add(DeserializeISerializable(type, variables, inputStream, objTracker, newInstance));
            }
            else
            {
                notTrackedExpressions.Add(Expression.IfThenElse(Expression.Equal(PrimitiveHelpers.ReadByte(inputStream, objTracker), Expression.Constant(1, typeof(int))),
                                                                DeserializeDictionary(type, variables, inputStream, objTracker, newInstance),
                                                                DeserializeISerializable(type, variables, inputStream, objTracker, newInstance)));
            }

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracker,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }

        private static Expression DeserializeDictionary(Type type,
                                                        List<ParameterExpression> variables,
                                                        ParameterExpression inputStream,
                                                        ParameterExpression objTracker,
                                                        ParameterExpression newInstance)
        {
            var dictionaryType = DictionaryHelper.GetDictionaryType(type);
            var keyType = dictionaryType.GetGenericArguments()[0];
            var valueType = dictionaryType.GetGenericArguments()[1];

            var i = Expression.Parameter(typeof(int), "i");
            var length = Expression.Parameter(typeof(int), "length");
            var key = Expression.Parameter(keyType, "key");
            var value = Expression.Parameter(dictionaryType.GetGenericArguments()[1], "value");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "type");
            var deserializer = Expression.Parameter(typeof(Func<Stream, DeserializerObjectTracker, object>), "deserializer");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");

            variables.Add(i);
            variables.Add(length);
            variables.Add(key);
            variables.Add(value);
            variables.Add(typeName);
            variables.Add(typeExpr);
            variables.Add(deserializer);
            variables.Add(typeHashCode);

            var loopExpressions = new List<Expression>();

            GetReadExpression(inputStream, objTracker, keyType, loopExpressions, key, typeExpr, typeName, typeHashCode, deserializer);
            GetReadExpression(inputStream, objTracker, valueType, loopExpressions, value, typeExpr, typeName, typeHashCode, deserializer);
            loopExpressions.Add(Expression.Call(newInstance, DictionaryMih.Add(dictionaryType, keyType, dictionaryType.GetGenericArguments()[1]), key, value));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(loopExpressions);
            var breakLabel = Expression.Label("breakLabel");
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)),
                                        breakLabel);

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Assign(length, PrimitiveHelpers.ReadInt32(inputStream, objTracker)));
            notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            notTrackedExpressions.Add(Expression.Assign(newInstance, Expression.New(type.GetConstructor(new Type[0]))));
            if (type.IsClass)
            {
                notTrackedExpressions.Add(Expression.Call(objTracker, DeserializerObjectTrackerMih.TrackedObject(), newInstance));
            }

            notTrackedExpressions.Add(loop);
            notTrackedExpressions.Add(newInstance);

            return Expression.Block(notTrackedExpressions);

        }

        private static Expression DeserializeISerializable(Type type,
                                                           List<ParameterExpression> variables,
                                                           ParameterExpression inputStream,
                                                           ParameterExpression objTracker,
                                                           ParameterExpression newInstance)
        {
            var length = Expression.Parameter(typeof(int), "length");
            var i = Expression.Parameter(typeof(int), "i");
            var key = Expression.Parameter(typeof(string), "key");
            var value = Expression.Parameter(typeof(object), "value");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "type");
            var deserializer = Expression.Parameter(typeof(Func<Stream, DeserializerObjectTracker, object>), "deserializer");
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
            variables.Add(typeHashCode);

            var loopExpressions = new List<Expression>();

            GetReadExpression(inputStream, objTracker, typeof(string), loopExpressions, key, typeExpr, typeName, typeHashCode, deserializer);
            GetReadExpression(inputStream, objTracker, typeof(object), loopExpressions, value, typeExpr, typeName, typeHashCode, deserializer);
            loopExpressions.Add(Expression.Call(si, SerializationInfoMih.AddValue(), key, value));
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
            notTrackedExpressions.Add(Expression.Assign(context, Expression.New(StreamingContextMih.Constructor(), Expression.Constant(StreamingContextStates.All))));
            notTrackedExpressions.Add(Expression.Assign(si, Expression.New(SerializationInfoMih.Constructor(), Expression.Constant(type), fc)));
            notTrackedExpressions.Add(Expression.Assign(length, PrimitiveHelpers.ReadInt32(inputStream, objTracker)));
            notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            notTrackedExpressions.Add(loop);
            notTrackedExpressions.Add(Expression.Assign(newInstance, Expression.New(ISerializableSerializer.GetSerializationConstructor(type), si, context)));
            if (type.IsClass)
            {
                notTrackedExpressions.Add(Expression.Call(objTracker, DeserializerObjectTrackerMih.TrackedObject(), newInstance));
            }

            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializedAttributeExpression(type, newInstance, context));
            notTrackedExpressions.Add(SerializationCallbacksHelper.GenerateCallIDeserializationExpression(type, newInstance));
            notTrackedExpressions.Add(newInstance);

            return Expression.Block(notTrackedExpressions);
        }

        private static void GetReadExpression(ParameterExpression inputStream,
                                              ParameterExpression objTracking,
                                              Type expectedType,
                                              List<Expression> loopExpressions,
                                              ParameterExpression tmpVariable,
                                              ParameterExpression typeExpr,
                                              ParameterExpression typeName,
                                              ParameterExpression typeHashCode,
                                              ParameterExpression deserializer)
        {
            if (expectedType == typeof(string))
            {
                loopExpressions.Add(Expression.Assign(tmpVariable, Deserializer.GenerateStringExpression(inputStream, objTracking)));
            }
            else if (expectedType.IsPrimitive || expectedType.IsValueType)
            {
                var primitiveDeserializer = Deserializer.GetTypeDeserializer(expectedType);
                loopExpressions.Add(Expression.Assign(tmpVariable, Expression.Convert(Expression.Invoke(Expression.Constant(primitiveDeserializer), inputStream, objTracking), expectedType)));
            }
            else
            {
                loopExpressions.Add(Deserializer.GetReadClassExpression(inputStream, objTracking, tmpVariable, typeExpr, typeName, typeHashCode, deserializer, expectedType));
            }
        }
    }
}
