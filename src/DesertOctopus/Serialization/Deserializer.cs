﻿using System;
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
using DesertOctopus.Exceptions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Deserialization engine
    /// </summary>
    internal static class Deserializer
    {
        private static readonly ConcurrentDictionary<Type, Delegate> TypeDeserializers = new ConcurrentDictionary<Type, Delegate>();
        private static readonly ConcurrentDictionary<Type, Func<Stream, DeserializerObjectTracker, object>> TypeToObjectDeserializers = new ConcurrentDictionary<Type, Func<Stream, DeserializerObjectTracker, object>>();
        private static readonly Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, ParameterExpression, Expression>>> LazyPrimitiveMap = new Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, ParameterExpression, Expression>>>(BuildPrimitiveMap);

        /// <summary>
        /// Deserialize a byte array to create an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="bytes">Byte array that contains the object to be deserialized</param>
        /// <returns>The deserialized object</returns>
        public static T Deserialize<T>(byte[] bytes)
        {
            return Deserialize<T>(bytes,
                                  Serializer.DefaultOptions);
        }

        /// <summary>
        /// Deserialize a byte array to create an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="bytes">Byte array that contains the object to be deserialized</param>
        /// <param name="options">Serialization options</param>
        /// <returns>The deserialized object</returns>
        public static T Deserialize<T>(byte[] bytes, SerializationOptions options)
        {
            var obj = Deserialize(bytes,
                                  options,
                                  typeof(T));
            if (obj == null)
            {
                return default(T);
            }

            return (T)obj;
        }

        /// <summary>
        /// Deserialize a byte array to create an object
        /// </summary>
        /// <param name="bytes">Byte array that contains the object to be deserialized</param>
        /// <returns>The deserialized object</returns>
        public static object Deserialize(byte[] bytes)
        {
            return Deserialize(bytes,
                               Serializer.DefaultOptions,
                               null);
        }

        private static object Deserialize(byte[] bytes, SerializationOptions options, Type omittedRootType)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            if (options.OmitRootTypeName
                && omittedRootType == null)
            {
                throw new InvalidOmitRootTypeException("Omit root type name was set to true while its type was not provided for deserialization.");
            }

            using (var ms = new MemoryStream(bytes))
            {
                var tracker = new DeserializerObjectTracker();

                ValidateHeader(ms, tracker, options);

                TypeWithHashCode type;
                if (options.OmitRootTypeName)
                {
                    type = SerializedTypeResolver.GetTypeFromFullName(omittedRootType);
                }
                else
                {
                    var stringDeserializerMethod = (Func<Stream, DeserializerObjectTracker, string>)GetTypeDeserializer(typeof(string));

                    var typeName = stringDeserializerMethod(ms, tracker);
                    type = SerializedTypeResolver.GetTypeFromFullName(typeName);

                    if (type == null
                        || type.Type == null)
                    {
                        throw new TypeNotFoundException("Unknown type: " + typeName);
                    }
                }

                var intDeserializerMethod = (Func<Stream, DeserializerObjectTracker, int>)GetTypeDeserializer(typeof(int));
                int hashCode = intDeserializerMethod(ms, tracker);
                if (hashCode != type.HashCode)
                {
                    throw new TypeWasModifiedSinceSerializationException(type);
                }

                Func<Stream, DeserializerObjectTracker, object> deserializerMethod = GetTypeToObjectDeserializer(type.Type);
                object value = deserializerMethod(ms, tracker);

                Debug.Assert(ms.Position == ms.Length, "Byte array was not read completely." + ms.Position + " / " + ms.Length);
                Debug.Assert(value != null, "unable to deserialize?");

                return value;
            }
        }

        private static void ValidateHeader(MemoryStream ms,
                                           DeserializerObjectTracker objTracker,
                                           SerializationOptions options)
        {
            var int16Reader = (Func<Stream, DeserializerObjectTracker, short>)GetTypeDeserializer(typeof(short));
            var byteReader = (Func<Stream, DeserializerObjectTracker, byte>)GetTypeDeserializer(typeof(byte));
            short version = int16Reader(ms, objTracker);
            byte ort = byteReader(ms, objTracker);

            if (version != InternalSerializationStuff.Version)
            {
                throw new InvalidSerializationVersionException(string.Format("Wrong serialization version. Was {0} and we expected {1}",
                                                                             version,
                                                                             InternalSerializationStuff.Version));
            }

            if (options.OmitRootTypeName && ort != 1)
            {
                throw new InvalidOmitRootTypeException("Omit root type name was set to true while the deserialization header has it set to false");
            }


        }

        /// <summary>
        /// Empty the deserializer cache
        /// </summary>
        public static void ClearTypeDeserializersCache()
        {
            TypeDeserializers.Clear();
        }

        /// <summary>
        /// Get a deserializer from its type
        /// </summary>
        /// <param name="type">Type to deserialize</param>
        /// <returns>The deserializer related to the type parameter</returns>
        internal static Func<Stream, DeserializerObjectTracker, object> GetTypeToObjectDeserializer(Type type)
        {
            return TypeToObjectDeserializers.GetOrAdd(type,
                                                      t =>
                                                      {
                                                          var del = GetTypeDeserializer(t);

                                                          var inputStream = Expression.Parameter(typeof(Stream), "inputStream");
                                                          var objTracking = Expression.Parameter(typeof(DeserializerObjectTracker), "objTracking");
                                                          var block = Expression.Block(Expression.Convert(Expression.Invoke(Expression.Constant(del), inputStream, objTracking), typeof(object)));

                                                          return Expression.Lambda<Func<Stream, DeserializerObjectTracker, object>>(block, inputStream, objTracking).Compile();
                                                      });
        }

        internal static System.Delegate GetTypeDeserializer(Type type)
        {
            return TypeDeserializers.GetOrAdd(type, CreateTypeDeserializer);
        }

        private static ConcurrentDictionary<Type, Func<ParameterExpression, ParameterExpression, Expression>> BuildPrimitiveMap()
        {
            var map = new ConcurrentDictionary<Type, Func<ParameterExpression, ParameterExpression, Expression>>();

            map.TryAdd(typeof(char), PrimitiveHelpers.ReadChar);
            map.TryAdd(typeof(char?), PrimitiveHelpers.ReadNullableChar);

            map.TryAdd(typeof(bool), PrimitiveHelpers.ReadBool);
            map.TryAdd(typeof(bool?), PrimitiveHelpers.ReadNullableBool);

            map.TryAdd(typeof(byte), PrimitiveHelpers.ReadByte);
            map.TryAdd(typeof(byte?), PrimitiveHelpers.ReadNullableByte);
            map.TryAdd(typeof(sbyte), PrimitiveHelpers.ReadSByte);
            map.TryAdd(typeof(sbyte?), PrimitiveHelpers.ReadNullableSByte);

            map.TryAdd(typeof(short), PrimitiveHelpers.ReadInt16);
            map.TryAdd(typeof(short?), PrimitiveHelpers.ReadNullableInt16);
            map.TryAdd(typeof(ushort), PrimitiveHelpers.ReadUInt16);
            map.TryAdd(typeof(ushort?), PrimitiveHelpers.ReadNullableUInt16);

            map.TryAdd(typeof(int), PrimitiveHelpers.ReadInt32);
            map.TryAdd(typeof(int?), PrimitiveHelpers.ReadNullableInt32);
            map.TryAdd(typeof(uint), PrimitiveHelpers.ReadUInt32);
            map.TryAdd(typeof(uint?), PrimitiveHelpers.ReadNullableUInt32);

            map.TryAdd(typeof(long), PrimitiveHelpers.ReadInt64);
            map.TryAdd(typeof(long?), PrimitiveHelpers.ReadNullableInt64);
            map.TryAdd(typeof(ulong), PrimitiveHelpers.ReadUInt64);
            map.TryAdd(typeof(ulong?), PrimitiveHelpers.ReadNullableUInt64);

            map.TryAdd(typeof(double), PrimitiveHelpers.ReadDouble);
            map.TryAdd(typeof(double?), PrimitiveHelpers.ReadNullableDouble);
            map.TryAdd(typeof(decimal), PrimitiveHelpers.ReadDecimal);
            map.TryAdd(typeof(decimal?), PrimitiveHelpers.ReadNullableDecimal);
            map.TryAdd(typeof(float), PrimitiveHelpers.ReadSingle);
            map.TryAdd(typeof(float?), PrimitiveHelpers.ReadNullableSingle);
            map.TryAdd(typeof(DateTime), PrimitiveHelpers.ReadDateTime);
            map.TryAdd(typeof(DateTime?), PrimitiveHelpers.ReadNullableDateTime);
            map.TryAdd(typeof(TimeSpan), PrimitiveHelpers.ReadTimeSpan);
            map.TryAdd(typeof(TimeSpan?), PrimitiveHelpers.ReadNullableTimeSpan);
            map.TryAdd(typeof(BigInteger), PrimitiveHelpers.ReadBigInteger);
            map.TryAdd(typeof(BigInteger?), PrimitiveHelpers.ReadNullableBigInteger);

            return map;
        }

        internal static Func<ParameterExpression, ParameterExpression, Expression> GetPrimitiveReader(Type type)
        {
            Func<ParameterExpression, ParameterExpression, Expression> reader;
            if (LazyPrimitiveMap.Value.TryGetValue(type, out reader))
            {
                return reader;
            }

            if (type.IsEnum
                && LazyPrimitiveMap.Value.TryGetValue(type.GetEnumUnderlyingType(),
                                                      out reader))
            {
                return reader;
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null
                && underlyingType.IsEnum)
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(underlyingType.GetEnumUnderlyingType());
                if (LazyPrimitiveMap.Value.TryGetValue(nullableType, out reader))
                {
                    return reader;
                }
            }

            return null;
        }

        private static System.Delegate CreateTypeDeserializer(Type type)
        {
            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var inputStream = Expression.Parameter(typeof(Stream), "inputStream");
            var objTracker = Expression.Parameter(typeof(DeserializerObjectTracker), "objTracker");
            var returnValue = Expression.Parameter(type, "retVal");

            variables.Add(returnValue);

            var primitiveReader = GetPrimitiveReader(type);
            if (primitiveReader != null)
            {
                expressions.Add(Expression.Assign(returnValue, Expression.Convert(primitiveReader(inputStream, objTracker), type)));
            }
            else if (type == typeof(string))
            {
                expressions.Add(Expression.Assign(returnValue, GenerateStringExpression(inputStream, objTracker)));
            }
            else if (InternalSerializationStuff.ImplementsISerializableWithSerializationConstructor(type))
            {
                expressions.Add(Expression.Assign(returnValue, ISerializableDeserializer.GenerateISerializableExpression(type, variables, inputStream, objTracker)));
            }
            else if (InternalSerializationStuff.ImplementsDictionaryGeneric(type))
            {
                expressions.Add(Expression.Assign(returnValue, DictionaryDeserializer.GenerateDictionaryGenericExpression(type, variables, inputStream, objTracker)));
            }
            else if (type.IsArray)
            {
                expressions.Add(Expression.Assign(returnValue, GenerateArrayExpression(type, inputStream, objTracker)));
            }
            else if (type == typeof(ExpandoObject))
            {
                expressions.Add(Expression.Assign(returnValue, ExpandoDeserializer.GenerateExpandoObjectExpression(variables, inputStream, objTracker)));
            }
            else
            {
                expressions.Add(Expression.Assign(returnValue, GenerateClassExpression(type, inputStream, objTracker)));
            }

            expressions.Add(returnValue);

            var block = Expression.Block(variables, expressions);
            var f = typeof(Func<,,>).MakeGenericType(typeof(Stream), typeof(DeserializerObjectTracker), type);
            return Expression.Lambda(f, block, inputStream, objTracker).Compile();
        }

        private static Expression GenerateArrayExpression(Type type,
                                                          ParameterExpression inputStream,
                                                          ParameterExpression objTracking)
        {
            var elementType = type.GetElementType();

            if (elementType.IsArray)
            {
                return JaggedArrayDeserializer.GenerateJaggedArray(type,
                                                                   inputStream,
                                                                   objTracking);
            }
            else
            {
                return ArrayDeserializer.GenerateArrayOfKnownDimension(type,
                                                                       inputStream,
                                                                       objTracking);
            }
        }

        /// <summary>
        /// Generate an expression tree to deserialize a class
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Reference tracker</param>
        /// <param name="leftSide">Left side of the assignment</param>
        /// <param name="typeExpr">Type of the class as an expression</param>
        /// <param name="typeName">Type of the class as a string</param>
        /// <param name="typeHashCode">Hashcode of the class</param>
        /// <param name="deserializer">Temporary deserializer variable</param>
        /// <param name="itemType">Type of the class</param>
        /// <returns>An expression tree to deserialize a class</returns>
        internal static Expression GetReadClassExpression(ParameterExpression inputStream,
                                                          ParameterExpression objTracker,
                                                          Expression leftSide,
                                                          ParameterExpression typeExpr,
                                                          ParameterExpression typeName,
                                                          ParameterExpression typeHashCode,
                                                          ParameterExpression deserializer,
                                                          Type itemType)
        {
            var readType = Expression.IfThenElse(Expression.Equal(Expression.Convert(PrimitiveHelpers.ReadByte(inputStream, objTracker), typeof(byte)), Expression.Constant(SerializerObjectTracker.Value0)),
                                                               Expression.Assign(typeExpr, Expression.Call(SerializedTypeResolverMih.GetTypeFromFullName_Type(), Expression.Constant(itemType))),
                                                               Expression.Block(Expression.Assign(typeName, Deserializer.GenerateStringExpression(inputStream, objTracker)),
                                                                                Expression.Assign(typeHashCode, PrimitiveHelpers.ReadInt32(inputStream, objTracker)),
                                                                                Expression.Assign(typeExpr, Expression.Call(SerializedTypeResolverMih.GetTypeFromFullName_String(), typeName)),
                                                                                Expression.IfThen(Expression.NotEqual(typeHashCode, Expression.Property(typeExpr, "HashCode")),
                                                                                                  Expression.Throw(Expression.New(TypeWasModifiedSinceSerializationException.GetConstructor(), typeExpr)))));

            var invokeDeserializer = Expression.Invoke(deserializer, inputStream, objTracker);
            Expression convertExpression = Expression.Convert(Expression.Call(SerializerMih.ConvertObjectToExpectedType(), invokeDeserializer, Expression.Constant(itemType)), itemType);

            return Expression.IfThenElse(Expression.Equal(Expression.Convert(PrimitiveHelpers.ReadByte(inputStream, objTracker), typeof(byte)), Expression.Constant(SerializerObjectTracker.Value0)),
                                                          Expression.Assign(leftSide, Expression.Constant(null, itemType)),
                                                          Expression.Block(
                                                                            readType,
                                                                            Expression.Assign(deserializer, Expression.Call(DeserializerMih.GetTypeToObjectDeserializer(), Expression.Property(typeExpr, "Type"))),
                                                                            Expression.Assign(leftSide, convertExpression)));

        }

        /// <summary>
        /// Convert an object to an IQueryable
        /// </summary>
        /// <param name="instance">Object to convert</param>
        /// <param name="expectedQueryableType">Expected type</param>
        /// <returns>An IQueryable</returns>
        internal static object ConvertObjectToIQueryable(object instance, Type expectedQueryableType)
        {
            if (instance == null)
            {
                return null;
            }

            Debug.Assert(typeof(IQueryable).IsAssignableFrom(expectedQueryableType), "Type is not assignable");
            Debug.Assert(instance.GetType().IsArray, "Type must be an array");

            var elementType = instance.GetType().GetElementType();
            var m = QueryableMih.AsQueryable(elementType);

            return m.Invoke(null, new[] { instance });
        }

        private static Expression GenerateClassExpression(Type type,
                                                          ParameterExpression inputStream,
                                                          ParameterExpression objTracker)
        {
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var trackType = Expression.Parameter(typeof(byte), "isAlreadyTracked");
            var newInstance = Expression.Parameter(type, "newInstance");
            var deserializer = Expression.Parameter(typeof(Func<Stream, DeserializerObjectTracker, object>), "deserializer");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "typeExpr");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");
            variables.Add(trackType);
            variables.Add(newInstance);
            variables.Add(deserializer);
            variables.Add(typeExpr);
            variables.Add(typeName);
            variables.Add(typeHashCode);
            var temporaryVariables = new Dictionary<Type, ParameterExpression>();

            List<Expression> notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Assign(newInstance, Expression.Convert(Expression.Call(FormatterServicesMih.GetUninitializedObject(), Expression.Constant(type)), type)));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializingAttributeExpression(type, newInstance, Expression.New(StreamingContextMih.Constructor(), Expression.Constant(StreamingContextStates.All))));

            if (type.IsClass)
            {
                notTrackedExpressions.Add(Expression.Call(objTracker, DeserializerObjectTrackerMih.TrackedObject(), newInstance));
            }

            GenerateReadFieldsExpression(type,
                                         InternalSerializationStuff.GetFields(type),
                                         inputStream,
                                         objTracker,
                                         temporaryVariables,
                                         variables,
                                         newInstance,
                                         notTrackedExpressions,
                                         typeExpr,
                                         typeName,
                                         typeHashCode,
                                         deserializer);

            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializedAttributeExpression(type, newInstance, Expression.New(StreamingContextMih.Constructor(), Expression.Constant(StreamingContextStates.All))));
            notTrackedExpressions.Add(SerializationCallbacksHelper.GenerateCallIDeserializationExpression(type, newInstance));

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracker,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }

        internal static void GenerateReadFieldsExpression(Type type,
                                                          FieldInfo[] fields,
                                                          ParameterExpression inputStream,
                                                          ParameterExpression objTracker,
                                                          Dictionary<Type, ParameterExpression> temporaryVariables,
                                                          List<ParameterExpression> variables,
                                                          ParameterExpression newInstance,
                                                          List<Expression> notTrackedExpressions,
                                                          ParameterExpression typeExpr,
                                                          ParameterExpression typeName,
                                                          ParameterExpression typeHashCode,
                                                          ParameterExpression deserializer)
        {
            Func<Type, ParameterExpression> getTempVar = t =>
                                                         {
                                                             ParameterExpression tmpVar;
                                                             if (!temporaryVariables.TryGetValue(t,
                                                                                                 out tmpVar))
                                                             {
                                                                 tmpVar = Expression.Parameter(t);
                                                                 temporaryVariables.Add(t,
                                                                                        tmpVar);
                                                                 variables.Add(tmpVar);
                                                             }

                                                             return tmpVar;
                                                         };


            foreach (var fieldInfo in fields)
            {
                var fieldValueExpr = Expression.Field(newInstance,
                                                      fieldInfo);

                if (fieldInfo.FieldType == typeof(string))
                {
                    if (fieldInfo.IsInitOnly)
                    {
                        var tmpVar = getTempVar(fieldInfo.FieldType);
                        notTrackedExpressions.Add(Expression.Assign(tmpVar, GenerateStringExpression(inputStream, objTracker)));
                        notTrackedExpressions.Add(Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
                                                                  Expression.Constant(fieldInfo),
                                                                  Expression.Convert(tmpVar,
                                                                                     typeof(object)),
                                                                  newInstance));
                    }
                    else
                    {
                        var tmpVar = getTempVar(fieldInfo.FieldType);
                        notTrackedExpressions.Add(Expression.Assign(tmpVar, GenerateStringExpression(inputStream, objTracker)));
                        notTrackedExpressions.Add(Expression.Assign(fieldValueExpr, tmpVar));
                    }
                }
                else if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType.IsValueType)
                {
                    var primitiveReader = GetPrimitiveReader(fieldInfo.FieldType);
                    Expression newValue;

                    if (primitiveReader == null)
                    {
                        var primitiveDeserializer = GetTypeDeserializer(fieldInfo.FieldType);
                        newValue = Expression.Convert(Expression.Invoke(Expression.Constant(primitiveDeserializer), inputStream, objTracker), fieldInfo.FieldType);
                    }
                    else
                    {
                        newValue = primitiveReader(inputStream, objTracker);

                        if (fieldInfo.FieldType == typeof(byte)
                            || fieldInfo.FieldType == typeof(sbyte)
                            || fieldInfo.FieldType == typeof(byte?)
                            || fieldInfo.FieldType == typeof(sbyte?)
                            || IsEnumOrNullableEnum(fieldInfo.FieldType))
                        {
                            newValue = Expression.Convert(newValue, fieldInfo.FieldType);
                        }
                    }

                    var tmpVar = getTempVar(fieldInfo.FieldType);
                    notTrackedExpressions.Add(Expression.Assign(tmpVar,
                                                                newValue));

                    if (fieldInfo.IsInitOnly)
                    {
                        notTrackedExpressions.Add(Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(), Expression.Constant(fieldInfo), Expression.Convert(tmpVar, typeof(object)), newInstance));
                    }
                    else
                    {
                        notTrackedExpressions.Add(Expression.Assign(fieldValueExpr, tmpVar));
                    }
                }
                else
                {
                    if (fieldInfo.IsInitOnly)
                    {
                        var tmpVar = getTempVar(typeof(object));
                        notTrackedExpressions.Add(GetReadClassExpression(inputStream, objTracker, tmpVar, typeExpr, typeName, typeHashCode, deserializer, fieldInfo.FieldType));
                        notTrackedExpressions.Add(Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
                                                                  Expression.Constant(fieldInfo),
                                                                  tmpVar,
                                                                  newInstance));
                    }
                    else
                    {
                        notTrackedExpressions.Add(GetReadClassExpression(inputStream, objTracker, fieldValueExpr, typeExpr, typeName, typeHashCode, deserializer, fieldInfo.FieldType));
                    }
                }
            }
        }

        internal static bool IsEnumOrNullableEnum(Type type)
        {
            if (type.IsEnum)
            {
                return true;
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return underlyingType.IsEnum;
            }

            return false;
        }

        /// <summary>
        /// Generates an expression tree to read a string
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to read a string</returns>
        internal static Expression GenerateStringExpression(ParameterExpression inputStream,
                                                            ParameterExpression objTracking)
        {
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var trackType = Expression.Parameter(typeof(byte), "isAlreadyTracked");
            var newInstance = Expression.Parameter(typeof(string), "newInstance");
            var deserializer = Expression.Parameter(typeof(Func<Stream, DeserializerObjectTracker, object>), "deserializer");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "typeExpr");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");
            variables.Add(trackType);
            variables.Add(newInstance);
            variables.Add(deserializer);
            variables.Add(typeExpr);
            variables.Add(typeName);
            variables.Add(typeHashCode);

            List<Expression> notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Assign(newInstance, PrimitiveHelpers.ReadString(inputStream, objTracking)));
            notTrackedExpressions.Add(Expression.Call(objTracking, DeserializerObjectTrackerMih.TrackedObject(), newInstance));

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(typeof(string),
                                                                         inputStream,
                                                                         objTracking,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }

        /// <summary>
        /// Generate an expression to handle the 'is the object null' case
        /// </summary>
        /// <param name="type">Type of the object</param>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Reference tracker</param>
        /// <param name="newInstance">New deserialized object</param>
        /// <param name="notTrackedExpressions">Expressions to execute if the object is not tracked</param>
        /// <param name="trackType">Type of the tracked object</param>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <returns>An expression tree to handle the 'is the object null' case</returns>
        internal static Expression GenerateNullTrackedOrUntrackedExpression(Type type,
                                                                            ParameterExpression inputStream,
                                                                            ParameterExpression objTracker,
                                                                            ParameterExpression newInstance,
                                                                            List<Expression> notTrackedExpressions,
                                                                            ParameterExpression trackType,
                                                                            List<ParameterExpression> variables)
        {
            var alreadyTrackedExpr = Expression.Assign(newInstance, Expression.Convert(Expression.Call(objTracker, DeserializerObjectTrackerMih.GetTrackedObject(), PrimitiveHelpers.ReadInt32(inputStream, objTracker)), type));

            var notAlreadyTrackedExpr = Expression.Block(notTrackedExpressions);
            var isNotNullExpr = Expression.Block(Expression.Assign(trackType, Expression.Convert(PrimitiveHelpers.ReadByte(inputStream, objTracker), typeof(byte))),
                                                 Expression.IfThenElse(Expression.Equal(trackType, Expression.Constant(SerializerObjectTracker.Value0)),
                                                                       alreadyTrackedExpr,
                                                                       notAlreadyTrackedExpr));

            var expr = Expression.IfThenElse(Expression.Equal(Expression.Convert(PrimitiveHelpers.ReadByte(inputStream, objTracker), typeof(byte)), Expression.Constant(SerializerObjectTracker.Value0)),
                                             Expression.Assign(newInstance, Expression.Convert(Expression.Constant(null), type)),
                                             isNotNullExpr);

            return Expression.Block(variables, expr, newInstance);
        }
    }
}
