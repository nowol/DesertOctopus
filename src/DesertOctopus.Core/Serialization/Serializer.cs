using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Serialization engine
    /// </summary>
    internal static class Serializer
    {
        private static readonly ConcurrentDictionary<Type, System.Delegate> TypeSerializers = new ConcurrentDictionary<Type, System.Delegate>();
        private static readonly ConcurrentDictionary<Type, Action<Stream, object, SerializerObjectTracker>> TypeToObjectSerializers = new ConcurrentDictionary<Type, Action<Stream, object, SerializerObjectTracker>>();
        private static readonly Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression, Expression>>> LazyPrimitiveMap = new Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression, Expression>>>(BuildPrimitiveMap);
        internal static readonly SerializationOptions DefaultOptions = new SerializationOptions();

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Byte array containing the serialized object</returns>
        public static byte[] Serialize<T>(T obj)
        {
            return Serialize(obj,
                             DefaultOptions);
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="options">Serialization options</param>
        /// <returns>Byte array containing the serialized object</returns>
        public static byte[] Serialize<T>(T obj, SerializationOptions options)
        {
            using (var ms = new MemoryStream())
            {
                if (obj == null)
                {
                    return new byte[0];
                }

                object objToSerialize = ObjectCleaner.PrepareObjectForSerialization(obj);
                var cargo = new SerializerObjectTracker();


                var serializerMethod = GetTypeToObjectSerializer(obj.GetType());
                var stringSerializerMethod = (Action<Stream, string, SerializerObjectTracker>)GetTypeSerializer(typeof(string));
                var intSerializerMethod = (Action<Stream, int, SerializerObjectTracker>)GetTypeSerializer(typeof(int));

                WriteHeader(ms, cargo, options);

                if (!options.OmitRootTypeName)
                {
                    stringSerializerMethod(ms, SerializedTypeResolver.GetShortNameFromType(obj.GetType()), cargo);
                }

                intSerializerMethod(ms, SerializedTypeResolver.GetHashCodeFromType(obj.GetType()), cargo);
                serializerMethod(ms, objToSerialize, cargo);

                return ms.ToArray();
            }
        }

        private static void WriteHeader(MemoryStream ms,
                                        SerializerObjectTracker objectTracker,
                                        SerializationOptions options)
        {
            var shortSerializerMethod = (Action<Stream, short, SerializerObjectTracker>)GetTypeSerializer(typeof(short));
            var byteSerializerMethod = (Action<Stream, byte, SerializerObjectTracker>)GetTypeSerializer(typeof(byte));
            shortSerializerMethod(ms, InternalSerializationStuff.Version, objectTracker);
            byteSerializerMethod(ms, options.OmitRootTypeName ? (byte)1 : (byte)0, objectTracker);
        }

        /// <summary>
        /// Clear the serializer cache
        /// </summary>
        public static void ClearTypeSerializersCache()
        {
            TypeSerializers.Clear();
        }

        /// <summary>
        /// Get the serializer for the specified type
        /// </summary>
        /// <param name="type">Type to serialize</param>
        /// <returns>Serializer for the specified type</returns>
        internal static Action<Stream, object, SerializerObjectTracker> GetTypeToObjectSerializer(Type type)
        {
            return TypeToObjectSerializers.GetOrAdd(type,
                                                      t =>
                                                      {
                                                          var del = GetTypeSerializer(t);

                                                          var outputStream = Expression.Parameter(typeof(Stream), "outputStream");
                                                          var objToSerialize = Expression.Parameter(typeof(object), "objToSerialize");
                                                          var objTracking = Expression.Parameter(typeof(SerializerObjectTracker), "objTracking");
                                                          var block = Expression.Invoke(Expression.Constant(del), outputStream, Expression.Convert(objToSerialize, t), objTracking);

                                                          return Expression.Lambda<Action<Stream, object, SerializerObjectTracker>>(block, outputStream, objToSerialize, objTracking).Compile();
                                                      });
        }

        internal static System.Delegate GetTypeSerializer(Type type)
        {
            return TypeSerializers.GetOrAdd(type, CreateTypeSerializer);
        }

        private static ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression, Expression>> BuildPrimitiveMap()
        {
            var map = new ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression, Expression>>();

            map.TryAdd(typeof(char), PrimitiveHelpers.WriteChar);
            map.TryAdd(typeof(char?), PrimitiveHelpers.WriteNullableChar);

            map.TryAdd(typeof(bool), PrimitiveHelpers.WriteBool);
            map.TryAdd(typeof(bool?), PrimitiveHelpers.WriteNullableBool);

            map.TryAdd(typeof(byte), PrimitiveHelpers.WriteByte);
            map.TryAdd(typeof(byte?), PrimitiveHelpers.WriteNullableByte);
            map.TryAdd(typeof(sbyte), PrimitiveHelpers.WriteSByte);
            map.TryAdd(typeof(sbyte?), PrimitiveHelpers.WriteNullableSByte);
            map.TryAdd(typeof(short), PrimitiveHelpers.WriteInt16);
            map.TryAdd(typeof(short?), PrimitiveHelpers.WriteNullableInt16);
            map.TryAdd(typeof(ushort), PrimitiveHelpers.WriteUInt16);
            map.TryAdd(typeof(ushort?), PrimitiveHelpers.WriteNullableUInt16);
            map.TryAdd(typeof(int), PrimitiveHelpers.WriteInt32);
            map.TryAdd(typeof(int?), PrimitiveHelpers.WriteNullableInt32);
            map.TryAdd(typeof(uint), PrimitiveHelpers.WriteUInt32);
            map.TryAdd(typeof(uint?), PrimitiveHelpers.WriteNullableUInt32);
            map.TryAdd(typeof(long), PrimitiveHelpers.WriteInt64);
            map.TryAdd(typeof(long?), PrimitiveHelpers.WriteNullableInt64);
            map.TryAdd(typeof(ulong), PrimitiveHelpers.WriteUInt64);
            map.TryAdd(typeof(ulong?), PrimitiveHelpers.WriteNullableUInt64);

            map.TryAdd(typeof(double), PrimitiveHelpers.WriteDouble);
            map.TryAdd(typeof(double?), PrimitiveHelpers.WriteNullableDouble);
            map.TryAdd(typeof(decimal), PrimitiveHelpers.WriteDecimal);
            map.TryAdd(typeof(decimal?), PrimitiveHelpers.WriteNullableDecimal);
            map.TryAdd(typeof(float), PrimitiveHelpers.WriteSingle);
            map.TryAdd(typeof(float?), PrimitiveHelpers.WriteNullableSingle);
            map.TryAdd(typeof(DateTime), PrimitiveHelpers.WriteDateTime);
            map.TryAdd(typeof(DateTime?), PrimitiveHelpers.WriteNullableDateTime);
            map.TryAdd(typeof(TimeSpan), PrimitiveHelpers.WriteTimeSpan);
            map.TryAdd(typeof(TimeSpan?), PrimitiveHelpers.WriteNullableTimeSpan);
            map.TryAdd(typeof(BigInteger), PrimitiveHelpers.WriteBigInteger);
            map.TryAdd(typeof(BigInteger?), PrimitiveHelpers.WriteNullableBigInteger);

            return map;
        }

        internal static Func<ParameterExpression, Expression, Expression, Expression> GetPrimitiveWriter(Type type)
        {
            Func<ParameterExpression, Expression, Expression, Expression> writer;
            if (LazyPrimitiveMap.Value.TryGetValue(type, out writer))
            {
                return writer;
            }

            if (type.GetTypeInfo().IsEnum
                && LazyPrimitiveMap.Value.TryGetValue(Enum.GetUnderlyingType(type), out writer))
            {
                return writer;
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null
                && underlyingType.GetTypeInfo().IsEnum)
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(underlyingType));
                if (LazyPrimitiveMap.Value.TryGetValue(nullableType, out writer))
                {
                    return writer;
                }
            }

            return null;
        }

        private static System.Delegate CreateTypeSerializer(Type type)
        {
            InternalSerializationStuff.ValidateSupportedTypes(type);

            var variables = new List<ParameterExpression>();
            var expressions = new List<Expression>();
            var outputStream = Expression.Parameter(typeof(Stream), "outputStream");
            var objToSerialize = Expression.Parameter(type, "objToSerialize");
            var objTracking = Expression.Parameter(typeof(SerializerObjectTracker), "objTracking");

            var primitiveWriter = GetPrimitiveWriter(type);
            if (primitiveWriter != null)
            {
                Debug.Assert(type.GetTypeInfo().IsPrimitive || type.GetTypeInfo().IsValueType, "Value is not a primitive");
                expressions.Add(primitiveWriter(outputStream, objToSerialize, objTracking));
            }
            else if (type == typeof(string))
            {
                expressions.Add(GenerateStringExpression(outputStream, objToSerialize, objTracking));
            }
            else if (typeof(ISerializable).IsAssignableFrom(type) || options)
            {
                expressions.Add(ISerializableSerializer.GenerateISerializableExpression(type, variables, outputStream, objToSerialize, objTracking));
            }
            else if (type == typeof(ExpandoObject))
            {
                expressions.Add(ExpandoSerializer.GenerateExpandoObjectExpression(type, variables, outputStream, objToSerialize, objTracking));
            }
            else if (type.IsArray)
            {
                expressions.Add(GenerateArrayExpression(type, outputStream, objToSerialize, objTracking));
            }
            else
            {
                expressions.Add(GenerateClassExpression(type, outputStream, objToSerialize, objTracking));
            }

            var block = Expression.Block(variables, expressions);

            var a = typeof(Action<,,>).MakeGenericType(typeof(Stream), type, typeof(SerializerObjectTracker));
            return Expression.Lambda(a, block, outputStream, objToSerialize, objTracking).Compile();
        }

        /// <summary>
        /// Used by ResolveIEnumerable using reflection
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="items">Items to convert to an array</param>
        /// <returns>An array</returns>
        internal static T[] ConvertEnumerableToArray<T>(IEnumerable items)
        {
            return items.Cast<T>().ToArray();
        }

        private static Expression GenerateArrayExpression(Type type,
                                                          ParameterExpression outputStream,
                                                          ParameterExpression objToSerialize,
                                                          ParameterExpression objTracking)
        {
            var elementType = type.GetElementType();

            if (elementType.IsArray)
            {
                return JaggedArraySerializer.GenerateJaggedArray(type,
                                                                 elementType,
                                                                 outputStream,
                                                                 objToSerialize,
                                                                 objTracking);
            }
            else
            {
                return ArraySerializer.GenerateArrayOfKnownDimension(type,
                                                                     elementType,
                                                                     outputStream,
                                                                     objToSerialize,
                                                                     objTracking);
            }
        }

        /// <summary>
        /// Generates an expression tree to handle class type serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <param name="item">Source object</param>
        /// <param name="itemAsObj">Source object as an object</param>
        /// <param name="typeExpr">Type of the object as an expression</param>
        /// <param name="serializer">Serializer temporary variable</param>
        /// <param name="itemType">Type of the element</param>
        /// <returns>An expression tree to handle class type serialization</returns>
        internal static Expression GetWriteClassTypeExpression(ParameterExpression outputStream,
                                                              ParameterExpression objTracking,
                                                              Expression item,
                                                              Expression itemAsObj,
                                                              ParameterExpression typeExpr,
                                                              ParameterExpression serializer,
                                                              Type itemType)
        {
            var writeType = Expression.IfThenElse(Expression.Equal(Expression.Call(item, ObjectMih.GetTypeMethod()), Expression.Constant(itemType)),
                                                Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value0), objTracking),
                                                                 Expression.Assign(typeExpr, Expression.Constant(itemType)),
                                                                 Expression.Assign(itemAsObj, Expression.Convert(item, typeof(object)))),
                                                Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1), objTracking),
                                                                 Expression.Assign(itemAsObj, Expression.Call(SerializerMih.PrepareObjectForSerialization(), item)),
                                                                 Expression.Assign(typeExpr, Expression.Call(itemAsObj, ObjectMih.GetTypeMethod())),
                                                                 GenerateStringExpression(outputStream, Expression.Call(SerializedTypeResolverMih.GetShortNameFromType(), typeExpr), objTracking),
                                                                 PrimitiveHelpers.WriteInt32(outputStream, Expression.Call(SerializedTypeResolverMih.GetHashCodeFromType(), typeExpr), objTracking)));

            return Expression.IfThenElse(Expression.Equal(item, Expression.Constant(null)),
                                         PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value0), objTracking),
                                         Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1), objTracking),
                                                          writeType,
                                                          Expression.Assign(serializer, Expression.Call(SerializerMih.GetTypeToObjectSerializer(), typeExpr)),
                                                          Expression.Invoke(serializer, outputStream, itemAsObj, objTracking)));
        }

        /// <summary>
        /// Generate an epxression tree to handle class serialization
        /// </summary>
        /// <param name="type">Type to serialize</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An epxression tree to handle class serialization</returns>
        internal static Expression GenerateClassExpression(Type type,
                                                           ParameterExpression outputStream,
                                                           Expression objToSerialize,
                                                           ParameterExpression objTracking)
        {
            List<Expression> notTrackedExpressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var serializer = Expression.Parameter(typeof(Action<Stream, object, SerializerObjectTracker>), "serializer");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");

            variables.Add(serializer);
            variables.Add(typeExpr);
            variables.Add(itemAsObj);

            List<Expression> copyFieldsExpressions = new List<Expression>();
            if (type.GetTypeInfo().IsClass)
            {
                copyFieldsExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));
            }

            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializingAttributeExpression(type, objToSerialize, Expression.New(StreamingContextMih.Constructor(), Expression.Constant(StreamingContextStates.All))));

            foreach (var fieldInfo in InternalSerializationStuff.GetFields(type))
            {
                var fieldValueExpr = Expression.Field(Expression.Convert(objToSerialize, type), fieldInfo);

                if (fieldInfo.FieldType == typeof(string))
                {
                    copyFieldsExpressions.Add(GenerateStringExpression(outputStream, fieldValueExpr, objTracking));
                }
                else if (fieldInfo.FieldType.GetTypeInfo().IsPrimitive || fieldInfo.FieldType.GetTypeInfo().IsValueType)
                {
                    var primitiveWriter = GetPrimitiveWriter(fieldInfo.FieldType);

                    if (primitiveWriter == null)
                    {
                        var primitiveSerializer = GetTypeSerializer(fieldInfo.FieldType);
                        copyFieldsExpressions.Add(Expression.Invoke(Expression.Constant(primitiveSerializer), outputStream, fieldValueExpr, objTracking));
                    }
                    else
                    {
                        copyFieldsExpressions.Add(primitiveWriter(outputStream, fieldValueExpr, objTracking));
                    }
                }
                else
                {
                    copyFieldsExpressions.Add(GetWriteClassTypeExpression(outputStream, objTracking, fieldValueExpr, itemAsObj, typeExpr, serializer, fieldInfo.FieldType));
                }
            }

            notTrackedExpressions.AddRange(copyFieldsExpressions);
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializedAttributeExpression(type, objToSerialize, Expression.New(StreamingContextMih.Constructor(), Expression.Constant(StreamingContextStates.All))));

            return GenerateNullTrackedOrUntrackedExpression(type,
                                                            outputStream,
                                                            objToSerialize,
                                                            objTracking,
                                                            notTrackedExpressions,
                                                            variables);
        }

        /// <summary>
        /// Generates an expression tree to handle string serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to handle string serialization</returns>
        internal static Expression GenerateStringExpression(ParameterExpression outputStream,
                                                            Expression objToSerialize,
                                                            ParameterExpression objTracking)
        {
            List<Expression> notTrackedExpressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));
            notTrackedExpressions.Add(PrimitiveHelpers.WriteString(outputStream, objToSerialize, objTracking));

            return GenerateNullTrackedOrUntrackedExpression(typeof(string),
                                                            outputStream,
                                                            objToSerialize,
                                                            objTracking,
                                                            notTrackedExpressions,
                                                            variables);
        }

        /// <summary>
        /// Generates an expression to handle the 'is that object null' case
        /// </summary>
        /// <param name="type">Type of <paramref name="objToSerialize"/></param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <param name="notTrackedExpressions">Expressions that must be executed if the object is not tracked</param>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <returns>An expression to handle the 'is that object null' case</returns>
        internal static Expression GenerateNullTrackedOrUntrackedExpression(Type type,
                                                                            ParameterExpression outputStream,
                                                                            Expression objToSerialize,
                                                                            ParameterExpression objTracking,
                                                                            List<Expression> notTrackedExpressions,
                                                                            List<ParameterExpression> variables)
        {
            var trackedObjectPosition = Expression.Parameter(typeof(int?), "trackedObjectPosition");
            var canBeTracked = Expression.Parameter(typeof(bool), "canBeTracked");
            variables.Add(trackedObjectPosition);
            variables.Add(canBeTracked);

            var alreadyTrackedExpr = Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value0), objTracking),
                                                      PrimitiveHelpers.WriteInt32(outputStream, Expression.Convert(trackedObjectPosition, typeof(int)), objTracking));

            var isNullExpr = PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value0, typeof(byte)), objTracking);

            var isNotNullExpr = Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1), objTracking),
                                                 Expression.IfThenElse(Expression.IsTrue(canBeTracked),
                                                                       Expression.Assign(trackedObjectPosition, Expression.Call(objTracking, SerializerObjectTrackerMih.GetTrackedObjectIndex(), Expression.Convert(objToSerialize, typeof(object)))),
                                                                       Expression.Assign(trackedObjectPosition, Expression.Convert(Expression.Constant(null), typeof(int?)))),
                                                 Expression.IfThenElse(Expression.NotEqual(trackedObjectPosition, Expression.Constant(null, typeof(int?))),
                                                                       alreadyTrackedExpr,
                                                                       Expression.Block(new[] { PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1), objTracking) }.Concat(notTrackedExpressions))));

            List<Expression> expressions = new List<Expression>();
            expressions.Add(Expression.Assign(canBeTracked, Expression.Call(objTracking, SerializerObjectTrackerMih.CanBeTracked(), Expression.Constant(type))));
            expressions.Add(Expression.IfThenElse(Expression.AndAlso(Expression.IsTrue(canBeTracked), Expression.Equal(Expression.Convert(objToSerialize, typeof(object)), Expression.Constant(null))),
                                                     isNullExpr,
                                                     isNotNullExpr));

            return Expression.Block(variables, expressions);
        }
    }
}
