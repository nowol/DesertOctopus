using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using DesertOctopus.Exceptions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle ISerializable serialization
    /// </summary>
    internal static class ISerializableSerializer
    {
        /// <summary>
        /// Generates an expression tree to handle ISerializable serialization
        /// </summary>
        /// <param name="type">Type of the object to serialize</param>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to handle ISerializable serialization</returns>
        public static Expression GenerateISerializableExpression(Type type,
                                                                 List<ParameterExpression> variables,
                                                                 ParameterExpression outputStream,
                                                                 ParameterExpression objToSerialize,
                                                                 ParameterExpression objTracking)
        {
            if (GetSerializationConstructor(type) == null)
            {
                throw new MissingConstructorException("Cannot serialize type " + type + " because it does not have the required constructor for ISerializable.  If you inherits from a class that implements ISerializable you have to expose the serialization constructor.");
            }

            var dictionaryType = DictionaryHelper.GetDictionaryType(type, throwIfNotADictionary: false);
            var notTrackedExpressions = new List<Expression>();

            if (dictionaryType == null)
            {
                notTrackedExpressions.Add(SerializeISerializable(type, variables, outputStream, objToSerialize, objTracking));
            }
            else
            {
                notTrackedExpressions.Add(Expression.IfThenElse(Expression.IsTrue(Expression.Call(DictionaryMih.IsObjectADictionaryWithDefaultComparer(), objToSerialize)),
                                                                SerializeDictionary(type, variables, outputStream, objToSerialize, objTracking),
                                                                SerializeISerializable(type, variables, outputStream, objToSerialize, objTracking)));
            }

            return Serializer.GenerateNullTrackedOrUntrackedExpression(outputStream,
                                                                       objToSerialize,
                                                                       objTracking,
                                                                       notTrackedExpressions,
                                                                       variables);
        }

        private static Expression SerializeDictionary(Type type,
                                                      List<ParameterExpression> variables,
                                                      ParameterExpression outputStream,
                                                      ParameterExpression objToSerialize,
                                                      ParameterExpression objTracking)
        {
            MethodInfo getEnumeratorMethodInfo = type.GetMethod("GetEnumerator");
            var dictionaryType = DictionaryHelper.GetDictionaryType(type);
            var enumeratorMethod = Expression.Call(Expression.Convert(objToSerialize, type), getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo();
            loopBodyCargo.EnumeratorType = getEnumeratorMethodInfo.ReturnType;
            loopBodyCargo.KvpType = typeof(KeyValuePair<,>).MakeGenericType(dictionaryType.GetGenericArguments());

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)1, typeof(byte))));
            notTrackedExpressions.Add(PrimitiveHelpers.WriteInt32(outputStream, Expression.Property(Expression.Convert(objToSerialize, dictionaryType), "Count")));
            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));

            notTrackedExpressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop(variables,
                                                                                  GetISerializableLoopBodyCargo(outputStream, objTracking, "Key", dictionaryType.GetGenericArguments()[0], "Value", dictionaryType.GetGenericArguments()[1]),
                                                                                  enumeratorMethod,
                                                                                  null,
                                                                                  loopBodyCargo));

            return Expression.Block(notTrackedExpressions);
        }

        private static Expression SerializeISerializable(Type type,
                                                         List<ParameterExpression> variables,
                                                         ParameterExpression outputStream,
                                                         ParameterExpression objToSerialize,
                                                         ParameterExpression objTracking)
        {
            var fc = Expression.Parameter(typeof(FormatterConverter), "fc");
            var context = Expression.Parameter(typeof(StreamingContext), "context");
            var si = Expression.Parameter(typeof(SerializationInfo), "si");
            var iser = Expression.Parameter(typeof(ISerializable), "iser");

            variables.Add(fc);
            variables.Add(context);
            variables.Add(si);
            variables.Add(iser);

            var getEnumeratorMethodInfo = SerializationInfoMih.GetEnumerator();

            var enumeratorMethod = Expression.Call(si, getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo();
            loopBodyCargo.EnumeratorType = typeof(SerializationInfoEnumerator);
            loopBodyCargo.KvpType = typeof(SerializationEntry);

            var preLoopActions = new List<Expression>();
            preLoopActions.Add(PrimitiveHelpers.WriteInt32(outputStream, Expression.Property(si, SerializationInfoMih.MemberCount())));

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant((byte)0, typeof(byte))));
            notTrackedExpressions.Add(Expression.Assign(fc, Expression.New(typeof(FormatterConverter))));
            notTrackedExpressions.Add(Expression.Assign(context, Expression.New(StreamingContextMih.Constructor(), Expression.Constant(StreamingContextStates.All))));
            notTrackedExpressions.Add(Expression.Assign(si, Expression.New(SerializationInfoMih.Constructor(), Expression.Constant(type), fc)));
            notTrackedExpressions.Add(Expression.Assign(iser, Expression.Convert(objToSerialize, typeof(ISerializable))));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializingAttributeExpression(type, objToSerialize, context));
            notTrackedExpressions.Add(Expression.Call(iser, ISerializableMih.GetObjectData(), si, context));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializedAttributeExpression(type, objToSerialize, context));
            notTrackedExpressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(si, "IsFullTypeNameSetExplicit")),
                                                        Expression.Throw(Expression.New(InvalidOperationExceptionMih.Constructor(), Expression.Constant("Changing the full type name for an ISerializable is not supported")))));
            notTrackedExpressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(si, "IsAssemblyNameSetExplicit")),
                                                        Expression.Throw(Expression.New(InvalidOperationExceptionMih.Constructor(), Expression.Constant("Changing the assembly name for an ISerializable is not supported")))));
            notTrackedExpressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop(variables,
                                                                                  GetISerializableLoopBodyCargo(outputStream, objTracking, "Name", typeof(string), "Value", typeof(object)),
                                                                                  enumeratorMethod,
                                                                                  preLoopActions,
                                                                                  loopBodyCargo));
            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));

            return Expression.Block(notTrackedExpressions);
        }

        /// <summary>
        /// Gets the serialization constructor for the specified type
        /// </summary>
        /// <param name="type">Type that we want the constructor from</param>
        /// <returns>The serialization constructor for the specified type</returns>
        internal static ConstructorInfo GetSerializationConstructor(Type type)
        {
            return type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(SerializationInfo), typeof(StreamingContext) }, new ParameterModifier[0]);
        }

        private static Func<EnumerableLoopBodyCargo, Expression> GetISerializableLoopBodyCargo(ParameterExpression outputStream, ParameterExpression objTracking, string keyPropertyName, Type keyType, string valuePropertyname, Type valueType)
        {
            Func<EnumerableLoopBodyCargo, Expression> loopBody = cargo =>
            {
                var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty(keyPropertyName));
                var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty(valuePropertyname));

                var writeKeyExpr = GetWriteExpression(outputStream, objTracking, keyType, keyExpression, cargo);
                var writeValueExpr = GetWriteExpression(outputStream, objTracking, valueType, valueExpression, cargo);

                return Expression.Block(writeKeyExpr,
                                        writeValueExpr);
            };

            return loopBody;
        }

        private static Expression GetWriteExpression(ParameterExpression outputStream,
                                                     ParameterExpression objTracking,
                                                     Type valueType,
                                                     MemberExpression valueExpression,
                                                     EnumerableLoopBodyCargo cargo)
        {
            if (valueType == typeof(string))
            {
                return Serializer.GenerateStringExpression(outputStream, valueExpression, objTracking);
            }

            if (valueType.IsPrimitive || valueType.IsValueType)
            {
                Action<Stream, object, SerializerObjectTracker> primitiveSerializer = Serializer.GetTypeSerializer(valueType);
                return Expression.Invoke(Expression.Constant(primitiveSerializer), outputStream, Expression.Convert(valueExpression, typeof(object)), objTracking);
            }

            return Serializer.GetWriteClassTypeExpression(outputStream, objTracking, valueExpression, cargo.ItemAsObj, cargo.TypeExpr, cargo.Serializer, valueType);
        }
    }
}
