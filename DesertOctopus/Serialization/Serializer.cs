using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DesertOctopus.Serialization.Helpers;

namespace DesertOctopus.Serialization
{
    internal static class Serializer
    {
        public static byte[] Serialize<T>(T obj)
            where T: class
        {
            using (var ms = new MemoryStream())
            {
                if (obj == null)
                {
                    return new byte[0];
                }

                object objToSerialize = PrepareObjectForSerialization(obj);
                var cargo = new SerializerObjectTracker();

                Action<Stream, object, SerializerObjectTracker> shortSerializerMethod = GetTypeSerializer(typeof(short));
                Action<Stream, object, SerializerObjectTracker> serializerMethod = GetTypeSerializer(obj.GetType());
                Action<Stream, object, SerializerObjectTracker> byteSerializerMethod = GetTypeSerializer(typeof(byte));
                Action<Stream, object, SerializerObjectTracker> stringSerializerMethod = GetTypeSerializer(typeof(string));
                Action<Stream, object, SerializerObjectTracker> intSerializerMethod = GetTypeSerializer(typeof(int));

                WriteHeader(objToSerialize, shortSerializerMethod, ms, cargo, byteSerializerMethod);
                stringSerializerMethod(ms, SerializedTypeResolver.GetShortNameFromType(obj.GetType()), cargo);
                intSerializerMethod(ms, SerializedTypeResolver.GetHashCodeFromType(obj.GetType()), cargo);
                serializerMethod(ms, objToSerialize, cargo);

                return ms.ToArray();
            }
        }

        private static void WriteHeader<T>(T obj,
                                           Action<Stream, object, SerializerObjectTracker> shortSerializerMethod,
                                           MemoryStream ms,
                                           SerializerObjectTracker objectTracker,
                                           Action<Stream, object, SerializerObjectTracker> byteSerializerMethod)
        {
            shortSerializerMethod(ms, InternalSerializationStuff.Version, objectTracker);
            if (obj.GetType().IsValueType || obj.GetType().IsPrimitive)
            {
                byteSerializerMethod(ms, InternalSerializationStuff.SerializationType.ValueType, objectTracker);
            }
            else
            {
                byteSerializerMethod(ms, InternalSerializationStuff.SerializationType.Class, objectTracker);
            }
        }

        public static void ClearTypeSerializersCache()
        {
            TypeSerializers.Clear();
        }

        private static readonly ConcurrentDictionary<Type, Action<Stream, object, SerializerObjectTracker>> TypeSerializers = new ConcurrentDictionary<Type, Action<Stream, object, SerializerObjectTracker>>();
        private static readonly Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression>>> LazyPrimitiveMap = new Lazy<ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression>>>(BuildPrimitiveMap);

        internal static Action<Stream, object, SerializerObjectTracker> GetTypeSerializer(Type type)
        {
            return TypeSerializers.GetOrAdd(type, CreateTypeSerializer);
        }

        private static ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression>> BuildPrimitiveMap()
        {
            var map = new ConcurrentDictionary<Type, Func<ParameterExpression, Expression, Expression>>();

            map.TryAdd(typeof(string), PrimitiveHelpers.WriteString);
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
            map.TryAdd(typeof(Decimal), PrimitiveHelpers.WriteDecimal);
            map.TryAdd(typeof(Decimal?), PrimitiveHelpers.WriteNullableDecimal);
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

        private static Func<ParameterExpression, Expression, Expression> GetPrimitiveWriter(Type type)
        {
            Func<ParameterExpression, Expression, Expression> writer;
            if (LazyPrimitiveMap.Value.TryGetValue(type, out writer))
            {
                return writer;
            }

            return null;
        }

        private static Action<Stream, object, SerializerObjectTracker> CreateTypeSerializer(Type type)
        {
            System.Diagnostics.Debug.WriteLine("CreateTypeSerializer " + type);

            var variables = new List<ParameterExpression>();
            var expressions = new List<Expression>();
            var outputStream = Expression.Parameter(typeof(Stream), "outputStream");
            var objToSerialize = Expression.Parameter(typeof(object), "objToSerialize");
            var objCargo = Expression.Parameter(typeof(SerializerObjectTracker), "objCargo");

            ValidateSupportedTypes(type);

            var primitiveWriter = GetPrimitiveWriter(type);
            if (primitiveWriter != null)
            {
                if (type.IsPrimitive || type.IsValueType)
                {
                    expressions.Add(primitiveWriter(outputStream, Expression.Unbox(objToSerialize, type)));
                }
                else
                {
                    expressions.Add(primitiveWriter(outputStream, objToSerialize));
                }
            }
            else if (typeof(ISerializable).IsAssignableFrom(type))
            {
                expressions.Add(ISerializableSerializer.GenerateISerializableExpression(type, variables, outputStream, objToSerialize, objCargo));
            }
            else if (type == typeof(ExpandoObject))
            {
                expressions.Add(ExpandoSerializer.GenerateExpandoObjectExpression(type, variables, outputStream, objToSerialize, objCargo));
            }
            else if (type.IsArray)
            {
                expressions.Add(GenerateArrayExpression(type, outputStream, objToSerialize, objCargo));
            }
            else if (type.IsValueType && !type.IsEnum && !type.IsPrimitive)
            {
                expressions.Add(GenerateClassExpression(type, outputStream, objToSerialize, objCargo));
            }
            else if (typeof(IQueryable).IsAssignableFrom(type))
            {
                var queryableInterface = type.GetInterfaces()
                                             .FirstOrDefault(t => t.IsGenericType
                                                                  && t.GetGenericTypeDefinition() == typeof(IQueryable<>)
                                                                  && t.GetGenericArguments()
                                                                      .Length == 1);
                if (queryableInterface != null)
                {
                    var genericArgumentType = queryableInterface.GetGenericArguments()[0];
                    expressions.Add(GenerateArrayExpression(genericArgumentType.MakeArrayType(), outputStream, objToSerialize, objCargo));
                }
                else
                {
                    throw new NotSupportedException(type.ToString());
                }

            }
            else // class, struct, etc
            {
                expressions.Add(GenerateClassExpression(type, outputStream, objToSerialize, objCargo));
            }

            var block = Expression.Block(variables, expressions);

            return Expression.Lambda<Action<Stream, object, SerializerObjectTracker>>(block, outputStream, objToSerialize, objCargo).Compile();
        }

        private static void ValidateSupportedTypes(Type type)
        {
            if (typeof(Expression).IsAssignableFrom(type))
            {
                throw new NotSupportedException(type.ToString());
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                throw new NotSupportedException(type.ToString());
            }

            if (type.IsPointer)
            {
                throw new NotSupportedException($"Pointer types such as {type} are not suported");
            }

            if (InternalSerializationStuff.GetFields(type).Any(x => x.FieldType.IsPointer))
            {
                throw new NotSupportedException($"Type {type} cannot contains fields that are pointers.");
            }

            //if (typeof(IQueryable).IsAssignableFrom(type))
            //{
            //    throw new NotSupportedException(type.ToString());
            //}

            if (Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                     && type.IsGenericType && type.Name.Contains("AnonymousType")
                     && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase)
                        ||
                        type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                    && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
            {
                throw new NotSupportedException(type.ToString());
            }
        }

        private static object PrepareObjectForSerialization(object objToPrepare)
        {
            var enumerableValue = objToPrepare as IEnumerable;
            if (enumerableValue != null)
            {
                var objectType = objToPrepare.GetType();
                if (objectType.IsArray
                    || typeof(IList).IsAssignableFrom(objectType)
                    || typeof(ICollection).IsAssignableFrom(objectType))
                {
                    return objToPrepare;
                }

                if (enumerableValue.GetType().DeclaringType == typeof(System.Linq.Enumerable)
                    || (!String.IsNullOrWhiteSpace(enumerableValue.GetType().Namespace) && enumerableValue.GetType().Namespace.StartsWith("System.Linq")))
                {
                    Type itemType = typeof(object);

                    var enumerableInterface = objectType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    if (enumerableInterface != null)
                    {
                        itemType = enumerableInterface.GetGenericArguments()[0];
                    }

                    var converter = SerializerMIH.ConvertEnumerableToArray(itemType);
                    return converter.Invoke(null,
                                            new object[]
                                            {
                                                enumerableValue
                                            });
                }
            }


            return objToPrepare;
        }

        /// <summary>
        /// Used by ResolveIEnumerable using reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        private static T[] ConvertEnumerableToArray<T>(IEnumerable items)
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
        
        internal static Expression GetWriteClassTypeExpression(ParameterExpression outputStream,
                                                              ParameterExpression objTracking,
                                                              Expression item,
                                                              Expression itemAsObj,
                                                              ParameterExpression typeExpr,
                                                              ParameterExpression serializer,
                                                              Type elementType)
        {
            var writeType = Expression.IfThenElse(Expression.Equal(Expression.Call(item, ObjectMIH.GetTypeMethod()), Expression.Constant(elementType)),
                                                  Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)0)),
                                                                   Expression.Assign(typeExpr, Expression.Constant(elementType)),
                                                                   Expression.Assign(itemAsObj, Expression.Convert(item, typeof(object)))),
                                                  Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)1)),
                                                                   Expression.Assign(itemAsObj, Expression.Call(SerializerMIH.PrepareObjectForSerialization(), item)),
                                                                   Expression.Assign(typeExpr, Expression.Call(itemAsObj, ObjectMIH.GetTypeMethod())),
                                                                   PrimitiveHelpers.WriteString(outputStream,  Expression.Call(SerializedTypeResolverMIH.GetShortNameFromType(), typeExpr)),
                                                                   PrimitiveHelpers.WriteInt32(outputStream, Expression.Call(SerializedTypeResolverMIH.GetHashCodeFromType(), typeExpr))));

            return Expression.IfThenElse(Expression.Equal(item, Expression.Constant(null)),
                                         PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)0)),
                                         Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)1)),
                                                          writeType,
                                                          Expression.Assign(serializer, Expression.Call(SerializerMIH.GetTypeSerializer(), typeExpr)),
                                                          Expression.Invoke(serializer, outputStream, itemAsObj, objTracking)));
        }

        internal static Expression GenerateClassExpression(Type type,
                                                           ParameterExpression outputStream,
                                                           Expression objToSerialize,
                                                           ParameterExpression objTracking)
        {
            List<Expression> expressions = new List<Expression>();
            List<Expression> notTrackedExpressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var serializer = Expression.Parameter(typeof(Action<Stream, object, SerializerObjectTracker>), "serializer");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");

            variables.Add(serializer);
            variables.Add(typeExpr);
            variables.Add(itemAsObj);

            List<Expression> copyFieldsExpressions = new List<Expression>();
            copyFieldsExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMIH.TrackObject(), objToSerialize));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializingAttributeExpression(type, objToSerialize, Expression.New(StreamingContextMIH.Constructor(), Expression.Constant(StreamingContextStates.All))));

            foreach (var fieldInfo in InternalSerializationStuff.GetFields(type))
            {
                if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType.IsValueType || fieldInfo.FieldType == typeof(string))
                {
                    Action<Stream, object, SerializerObjectTracker> primitiveSerializer = GetTypeSerializer(fieldInfo.FieldType);
                    copyFieldsExpressions.Add(Expression.Invoke(Expression.Constant(primitiveSerializer), outputStream, Expression.Convert(Expression.Field(Expression.Convert(objToSerialize, type), fieldInfo), typeof(object)), objTracking));
                }
                else
                {
                    copyFieldsExpressions.Add(GetWriteClassTypeExpression(outputStream, objTracking, Expression.Field(Expression.Convert(objToSerialize, type), fieldInfo), itemAsObj, typeExpr, serializer, fieldInfo.FieldType));
                }
            }

            notTrackedExpressions.AddRange(copyFieldsExpressions);
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializedAttributeExpression(type, objToSerialize, Expression.New(StreamingContextMIH.Constructor(), Expression.Constant(StreamingContextStates.All))));

            return GenerateNullTrackedOrUntrackedExpression(outputStream,
                                                            objToSerialize,
                                                            objTracking,
                                                            notTrackedExpressions,
                                                            expressions,
                                                            variables);
        }

        internal static Expression GenerateNullTrackedOrUntrackedExpression(ParameterExpression outputStream,
                                                                           Expression objToSerialize,
                                                                           ParameterExpression objTracking,
                                                                           List<Expression> notTrackedExpressions,
                                                                           List<Expression> expressions,
                                                                           List<ParameterExpression> variables)
        {
            var trackedObjectPosition = Expression.Parameter(typeof(int?), "trackedObjectPosition");
            variables.Add(trackedObjectPosition);

            var alreadyTrackedExpr = Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(0)),
                                                      PrimitiveHelpers.WriteInt32(outputStream, Expression.Convert(trackedObjectPosition, typeof(int))));

            var isNullExpr = PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte) 0, typeof(byte)));

            var isNotNullExpr = Expression.Block(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte) 1)),
                                                 Expression.Assign(trackedObjectPosition, Expression.Call(objTracking, SerializerObjectTrackerMIH.GetTrackedObjectIndex(), objToSerialize)), 
                                                 Expression.IfThenElse(Expression.NotEqual(trackedObjectPosition, Expression.Constant(null, typeof(int?))),
                                                                       alreadyTrackedExpr,
                                                                       Expression.Block(new [] { PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)1)) }.Concat(notTrackedExpressions))));

            var ifIsNullExpr = Expression.IfThenElse(Expression.Equal(objToSerialize, Expression.Constant(null)),
                                                     isNullExpr,
                                                     isNotNullExpr);
            expressions.Add(ifIsNullExpr);

            return Expression.Block(variables, expressions);
        }
    }
}
