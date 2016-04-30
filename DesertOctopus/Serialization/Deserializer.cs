using System;
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
using DesertOctopus.Serialization.Exceptions;
using DesertOctopus.Serialization.Helpers;

namespace DesertOctopus.Serialization
{
    public static class Deserializer
    {
        public static T Deserialize<T>(byte[] bytes)
            where T: class
        {
            if (bytes == null || bytes.Length == 0)
            {
                return default(T);
            }

            using (var ms = new MemoryStream(bytes))
            {
                var objs = new List<object>();

                ValidateHeader<T>(ms, objs);

                Func<Stream, List<object>, object> stringDeserializerMethod = GetTypeDeserializer(typeof(string));
                Func<Stream, List<object>, object> intDeserializerMethod = GetTypeDeserializer(typeof(int));

                var typeName = (string)stringDeserializerMethod(ms, objs);
                var type = SerializedTypeResolver.GetTypeFromFullName(typeName);

                if (type == null || type.Type == null)
                {
                    throw new TypeNotFoundException("Unknown type: " + typeName);
                }

                int hashCode = (int)intDeserializerMethod(ms, objs);
                if (hashCode != type.HashCode)
                {
                    throw new TypeWasModifiedSinceSerializationException(type);
                }

                Func<Stream, List<object>, object> deserializerMethod = GetTypeDeserializer(type.Type);

                object value = deserializerMethod(ms, objs);

                Debug.Assert(ms.Position == ms.Length);

                if (value == null && !(type.Type.IsValueType || type.Type.IsPrimitive))
                {
                    throw new Exception("unable to deserialize?");
                }
                else if (value == null)
                {
                    return default(T);
                }
                return (T) value;
            }
        }

        private static void ValidateHeader<T>(MemoryStream ms,
                                              List<object> objs)
        {
            var int16Reader = GetTypeDeserializer(typeof(short));
            var byteReader = GetTypeDeserializer(typeof(byte));
            short version = (short) int16Reader(ms, objs);
            InternalSerializationStuff.SerializationType serializationType = (InternalSerializationStuff.SerializationType)(byte) byteReader(ms, objs);

            if (version != InternalSerializationStuff.Version)
            {
                throw new Exception("wrong version?");
            }

            //if (serializationType == InternalSerializationStuff.SerializationType.ValueType
            //    && !(typeof(T).IsValueType || typeof(T).IsPrimitive))
            //{
            //    throw new Exception("wrong type?");
            //}
            //if (serializationType == InternalSerializationStuff.SerializationType.Class
            //    && !typeof(T).IsValueType
            //    && !typeof(T).IsPrimitive)
            //{
            //    throw new Exception("wrong type?");
            //}
        }

        private static readonly ConcurrentDictionary<Type, Func<Stream, List<object>, object>> TypeDeserializers = new ConcurrentDictionary<Type, Func<Stream, List<object>, object>>();
        private static readonly Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, Expression>>> LazyPrimitiveMap = new Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, Expression>>>(BuildPrimitiveMap);

        public static void ClearTypeDeserializersCache()
        {
            TypeDeserializers.Clear();
        }

        internal static Func<Stream, List<object>, object> GetTypeDeserializer(Type type)
        {
            return TypeDeserializers.GetOrAdd(type, CreateTypeDeserializer);
        }

        private static ConcurrentDictionary<Type, Func<ParameterExpression, Expression>> BuildPrimitiveMap()
        {
            var map = new ConcurrentDictionary<Type, Func<ParameterExpression, Expression>>();

            map.TryAdd(typeof(string), PrimitiveHelpers.ReadString);
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
            map.TryAdd(typeof(Decimal), PrimitiveHelpers.ReadDecimal);
            map.TryAdd(typeof(Decimal?), PrimitiveHelpers.ReadNullableDecimal);
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

        private static Func<ParameterExpression, Expression> GetPrimitiveReader(Type type)
        {
            Func<ParameterExpression, Expression> reader;
            if (LazyPrimitiveMap.Value.TryGetValue(type, out reader))
            {
                return reader;
            }
            return null;
        }

        private static Func<Stream, List<object>, object> CreateTypeDeserializer(Type type)
        {
            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var inputStream = Expression.Parameter(typeof(Stream), "inputStream");
            var objTracking = Expression.Parameter(typeof(List<object>), "objTracking");
            var returnValue = Expression.Parameter(typeof(object), "retVal");

            variables.Add(returnValue);

            var primitiveReader = GetPrimitiveReader(type);
            if (primitiveReader != null)
            {
                expressions.Add(Expression.Assign(returnValue, Expression.Convert(Expression.Convert(primitiveReader(inputStream), type), typeof(object))));
            }
            else if (typeof(ISerializable).IsAssignableFrom(type))
            {
                expressions.Add(Expression.Assign(returnValue, ISerializableDeserializer.GenerateISerializableExpression(type, variables, inputStream, objTracking)));
            }
            else if (type.IsArray)
            {
                expressions.Add(Expression.Assign(returnValue, GenerateArrayExpression(type, inputStream, objTracking)));
            }
            else if (type == typeof(ExpandoObject))
            {
                expressions.Add(Expression.Assign(returnValue, ExpandoDeserializer.GenerateExpandoObjectExpression(type, variables, inputStream, objTracking)));
            }
            else if (type.IsValueType && !type.IsEnum && !type.IsPrimitive)
            {
                expressions.Add(Expression.Assign(returnValue, Expression.Convert(GenerateClassExpression(type, inputStream, objTracking), typeof(object))));
            }
            else if (typeof(IQueryable).IsAssignableFrom(type))
            {
                var queryableInterface = type.GetInterfaces()
                                             .FirstOrDefault(t => t.IsGenericType
                                                                  && t.GetGenericTypeDefinition() == typeof(IQueryable<>)
                                                                  && t.GetGenericArguments().Length == 1);
                if (queryableInterface != null)
                {
                    var genericArgumentType = queryableInterface.GetGenericArguments()[0];
                    var deserializedValue = GenerateArrayExpression(genericArgumentType.MakeArrayType(), inputStream, objTracking);


                    var m = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { genericArgumentType }, Expression.Convert(deserializedValue, typeof(IEnumerable<>).MakeGenericType(genericArgumentType)));
                    expressions.Add(Expression.Assign(returnValue, Expression.Convert(m, typeof(object))));
                }
                else
                {
                    throw new NotSupportedException(type.ToString());
                }

            }
            else// if (type.IsClass)
            {
                expressions.Add(Expression.Assign(returnValue, Expression.Convert(GenerateClassExpression(type, inputStream, objTracking), typeof(object))));
            }
            //else
            //{
            //    throw new NotSupportedException(string.Format("Type {0} is not supported",
            //                                                  type));
            //}

            // todo return type stuff ?


            expressions.Add(returnValue);

            var block = Expression.Block(variables, expressions);
            return Expression.Lambda<Func<Stream, List<object>, object>>(block, inputStream, objTracking).Compile();
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

        internal static Expression GetReadClassExpression(ParameterExpression inputStream,
                                                             ParameterExpression objTracking,
                                                             Expression leftSize,
                                                             ParameterExpression typeExpr,
                                                             ParameterExpression typeName,
                                                             ParameterExpression typeHashCode,
                                                             ParameterExpression deserializer,
                                                             Type elementType)
        {
            var readType = Expression.IfThenElse(Expression.Equal(Expression.Convert(PrimitiveHelpers.ReadByte(inputStream), typeof(byte)), Expression.Constant((byte)0)),
                                                                  Expression.Assign(typeExpr, Expression.Call(typeof(SerializedTypeResolver).GetMethod("GetTypeFromFullName", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, new Type[] { typeof(Type) }, null), Expression.Constant(elementType))),
                                                                  Expression.Block(Expression.Assign(typeName, PrimitiveHelpers.ReadString(inputStream)),
                                                                                   Expression.Assign(typeHashCode, PrimitiveHelpers.ReadInt32(inputStream)),
                                                                                   Expression.Assign(typeExpr, Expression.Call(typeof(SerializedTypeResolver).GetMethod("GetTypeFromFullName", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, new Type[] { typeof(string) }, null), typeName)),
                                                                                   Expression.IfThen(Expression.NotEqual(typeHashCode, Expression.Property(typeExpr, "HashCode")),
                                                                                                     Expression.Throw(Expression.New(TypeWasModifiedSinceSerializationException.GetConstructor(), typeExpr))))
                                                );

            var invokeDeserializer = Expression.Invoke(deserializer, inputStream, objTracking);
            Expression convertExpression;

            if (typeof(IQueryable).IsAssignableFrom(elementType))
            {
                convertExpression = Expression.Convert(Expression.Call(typeof(Deserializer).GetMethod("ConvertObjectToIQueryable", BindingFlags.Static | BindingFlags.NonPublic), invokeDeserializer, Expression.Constant(elementType)), elementType);
            }
            else
            {
                convertExpression = Expression.Convert(invokeDeserializer, elementType);
            }
            

            return Expression.IfThenElse(Expression.Equal(Expression.Convert(PrimitiveHelpers.ReadByte(inputStream), typeof(byte)), Expression.Constant((byte)0)),
                                                          Expression.Assign(leftSize, Expression.Constant(null, elementType)),
                                                          Expression.Block(
                                                                            readType,
                                                                            Expression.Assign(deserializer, Expression.Call(typeof(Deserializer).GetMethod("GetTypeDeserializer", BindingFlags.Static | BindingFlags.NonPublic), Expression.Property(typeExpr, "Type"))),
                                                                            Expression.Assign(leftSize, convertExpression)
                                                                            )
                                                        );

        }

        private static object ConvertObjectToIQueryable(object instance, Type expectedQueryableType)
        {
            if (instance == null)
            {
                return null;
            }

            Debug.Assert(typeof(IQueryable).IsAssignableFrom(expectedQueryableType));
            Debug.Assert(instance.GetType().IsArray);

            var elementType = instance.GetType().GetElementType();

            var m = typeof(Queryable).GetMethod("AsQueryable", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IEnumerable<>).MakeGenericType(elementType) }, new ParameterModifier[0]);

            return m.Invoke(null, new[] { instance });
        }

        private static object GetTrackedObject(List<object> obj, int index)
        {
            return obj[index];
        } 

        private static Expression GenerateClassExpression(Type type,
                                                          ParameterExpression inputStream,
                                                          ParameterExpression objTracking)
        {
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var trackType = Expression.Parameter(typeof(byte), "isAlreadyTracked");
            var newInstance = Expression.Parameter(type, "newInstance");
            var deserializer = Expression.Parameter(typeof(Func<Stream, List<object>, object>), "deserializer");
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
            notTrackedExpressions.Add(Expression.Assign(newInstance, Expression.Convert(Expression.Call(typeof(FormatterServices).GetMethod("GetUninitializedObject", BindingFlags.Public | BindingFlags.Static), Expression.Constant(type)), type)));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializingAttributeExpression(type, newInstance, Expression.New(typeof(StreamingContext).GetConstructor(new[] { typeof(StreamingContextStates) }), Expression.Constant(StreamingContextStates.All))));

            if (type.IsClass)
            {
                notTrackedExpressions.Add(Expression.Call(objTracking, typeof(List<object>).GetMethod("Add"), newInstance));
            }
            else
            {
                notTrackedExpressions.Add(Expression.Call(objTracking, typeof(List<object>).GetMethod("Add"), Expression.Convert(newInstance, typeof(object))));
            }

            foreach (var fieldInfo in InternalSerializationStuff.GetFields(type))
            {
                if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType.IsValueType || fieldInfo.FieldType == typeof(string))
                {
                    Func<Stream, List<object>, object> primitiveDeserializer = GetTypeDeserializer(fieldInfo.FieldType);
                    var newValue = Expression.Convert(Expression.Invoke(Expression.Constant(primitiveDeserializer), inputStream, objTracking), fieldInfo.FieldType);

                    if (fieldInfo.IsInitOnly)
                    {
                        notTrackedExpressions.Add(Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
                                                                Expression.Constant(fieldInfo),
                                                                Expression.Convert(newValue, typeof(object)),
                                                                newInstance));
                    }
                    else
                    {
                        notTrackedExpressions.Add(Expression.Assign(Expression.Field(newInstance, fieldInfo), newValue));
                    }
                }
                else
                {
                    notTrackedExpressions.Add(GetReadClassExpression(inputStream, objTracking, Expression.Field(newInstance, fieldInfo), typeExpr, typeName, typeHashCode, deserializer, fieldInfo.FieldType));
                }
            }

            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializedAttributeExpression(type, newInstance, Expression.New(typeof(StreamingContext).GetConstructor(new[] { typeof(StreamingContextStates) }), Expression.Constant(StreamingContextStates.All))));
            notTrackedExpressions.Add(SerializationCallbacksHelper.GenerateCallIDeserializationExpression(type, newInstance));

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracking,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }

        internal static Expression GenerateNullTrackedOrUntrackedExpression(Type type,
                                                                            ParameterExpression inputStream,
                                                                            ParameterExpression objTracking,
                                                                            ParameterExpression newInstance,
                                                                            List<Expression> notTrackedExpressions,
                                                                            ParameterExpression trackType,
                                                                            List<ParameterExpression> variables)
        {
            var alreadyTrackedExpr = Expression.Assign(newInstance, Expression.Convert(Expression.Call(typeof(Deserializer).GetMethod("GetTrackedObject", BindingFlags.NonPublic | BindingFlags.Static), objTracking, PrimitiveHelpers.ReadInt32(inputStream)), type));

            var notAlreadyTrackedExpr = Expression.Block(notTrackedExpressions);
            var isNotNullExpr = Expression.Block(Expression.Assign(trackType, Expression.Convert(PrimitiveHelpers.ReadByte(inputStream), typeof(byte))),
                                                 Expression.IfThenElse(Expression.Equal(trackType, Expression.Constant((byte)0)),
                                                                       alreadyTrackedExpr,
                                                                       notAlreadyTrackedExpr));

            var expr = Expression.IfThenElse(Expression.Equal(Expression.Convert(PrimitiveHelpers.ReadByte(inputStream), typeof(byte)), Expression.Constant((byte)0)),
                                             Expression.Assign(newInstance, Expression.Convert(Expression.Constant(null), type)),
                                             isNotNullExpr);

            return Expression.Block(variables, expr, newInstance);
        }

        private static class CopyReadOnlyFieldMethodInfo
        {
            private static readonly MethodInfo Method = typeof(CopyReadOnlyFieldMethodInfo).GetMethod("CopyReadonlyField", BindingFlags.NonPublic | BindingFlags.Static);

            public static MethodInfo GetMethodInfo()
            {
                return Method;
            }

            private static void CopyReadonlyField(FieldInfo field, object value, object target)
            {
                // using reflection to copy readonly fields.  It's slower but it's the only choice
                field.SetValue(target, value);
            }
        }
    }
}
