﻿using System;
using System.Collections.Generic;
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

            var fc = Expression.Parameter(typeof(FormatterConverter), "fc");
            var context = Expression.Parameter(typeof(StreamingContext), "context");
            var si = Expression.Parameter(typeof(SerializationInfo), "si");
            var iser = Expression.Parameter(typeof(ISerializable), "iser");

            variables.Add(fc);
            variables.Add(context);
            variables.Add(si);
            variables.Add(iser);

            var getEnumeratorMethodInfo = SerializationInfoMIH.GetEnumerator();

            var enumeratorMethod = Expression.Call(si, getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo();
            loopBodyCargo.EnumeratorType = typeof(SerializationInfoEnumerator);
            loopBodyCargo.KvpType = typeof(SerializationEntry);

            var preLoopActions = new List<Expression>();
            preLoopActions.Add(PrimitiveHelpers.WriteInt32(outputStream, Expression.Property(si, SerializationInfoMIH.MemberCount())));

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Assign(fc, Expression.New(typeof(FormatterConverter))));
            notTrackedExpressions.Add(Expression.Assign(context, Expression.New(StreamingContextMIH.Constructor(), Expression.Constant(StreamingContextStates.All))));
            notTrackedExpressions.Add(Expression.Assign(si, Expression.New(SerializationInfoMIH.Constructor(), Expression.Constant(type), fc)));
            notTrackedExpressions.Add(Expression.Assign(iser, Expression.Convert(objToSerialize, typeof(ISerializable))));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializingAttributeExpression(type, objToSerialize, context));
            notTrackedExpressions.Add(Expression.Call(iser, ISerializableMIH.GetObjectData(), si, context));
            notTrackedExpressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializedAttributeExpression(type, objToSerialize, context));
            notTrackedExpressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(si, "IsFullTypeNameSetExplicit")),
                                                        Expression.Throw(Expression.New(InvalidOperationExceptionMIH.Constructor(), Expression.Constant("Changing the full type name for an ISerializable is not supported")))));
            notTrackedExpressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(si, "IsAssemblyNameSetExplicit")),
                                                        Expression.Throw(Expression.New(InvalidOperationExceptionMIH.Constructor(), Expression.Constant("Changing the assembly name for an ISerializable is not supported")))));
            notTrackedExpressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop<string, object, SerializationInfoEnumerator>(variables,
                                                                                                                               GetLoopBodyCargo(outputStream, objTracking),
                                                                                                                               enumeratorMethod,
                                                                                                                               preLoopActions,
                                                                                                                               loopBodyCargo));
            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMIH.TrackObject(), objToSerialize));

            return Serializer.GenerateNullTrackedOrUntrackedExpression(outputStream,
                                                                       objToSerialize,
                                                                       objTracking,
                                                                       notTrackedExpressions,
                                                                       variables);
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

        private static Func<EnumerableLoopBodyCargo, Expression> GetLoopBodyCargo(ParameterExpression outputStream, ParameterExpression objTracking)
        {
            Func<EnumerableLoopBodyCargo, Expression> loopBody = cargo =>
            {
                var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Name"));
                var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Value"));

                return Expression.Block(Serializer.GenerateStringExpression(outputStream, keyExpression, objTracking),
                                        Serializer.GetWriteClassTypeExpression(outputStream, objTracking, valueExpression, cargo.ItemAsObj, cargo.TypeExpr, cargo.Serializer, typeof(object)));
            };

            return loopBody;
        }
    }
}
