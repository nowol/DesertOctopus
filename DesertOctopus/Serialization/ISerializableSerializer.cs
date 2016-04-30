using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using DesertOctopus.Serialization.Exceptions;
using DesertOctopus.Serialization.Helpers;

namespace DesertOctopus.Serialization
{
    internal static class ISerializableSerializer
    {
        public static Expression GenerateISerializableExpression(Type type,
                                                                 List<ParameterExpression> variables,
                                                                 ParameterExpression outputStream,
                                                                 ParameterExpression objToSerialize,
                                                                 ParameterExpression objTracking)
        {
            // validate that the type has the expected serialization constructor
            /*
             
            if (! type has serialization constructor)
                throw exception

            si = new serializationinfo // create method for this, can probably be reused in Deserialization
            var obj = objToSerialize as ISerializable;


             
             
             */


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



            var getEnumeratorMethodInfo = typeof(SerializationInfo).GetMethod("GetEnumerator", BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public, null, new Type[0], new ParameterModifier[0]);
            if (getEnumeratorMethodInfo == null)
            {
                throw new Exception("Could not find GetEnumerator method.");
            }
            var enumeratorMethod = Expression.Call(si, getEnumeratorMethodInfo);


            var loopBodyCargo = new EnumerableLoopBodyCargo<string, object>();
            loopBodyCargo.EnumeratorType = typeof(SerializationInfoEnumerator);
            loopBodyCargo.KvpType = typeof(SerializationEntry);


            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(fc, Expression.New(typeof(FormatterConverter))));
            expressions.Add(Expression.Assign(context, Expression.New(typeof(StreamingContext).GetConstructor(new []{typeof(StreamingContextStates)}), Expression.Constant(StreamingContextStates.All))));
            expressions.Add(Expression.Assign(si, Expression.New(typeof(SerializationInfo).GetConstructor(new []{typeof(Type), typeof(FormatterConverter) }), Expression.Constant(type), fc)));
            expressions.Add(Expression.Assign(iser, Expression.Convert(objToSerialize, typeof(ISerializable))));

            expressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializingAttributeExpression(type, objToSerialize, context));
            expressions.Add(Expression.Call(iser, typeof(ISerializable).GetMethod("GetObjectData", new [] { typeof(SerializationInfo), typeof(StreamingContext) }), si, context));
            expressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializedAttributeExpression(type, objToSerialize, context));

            expressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(si, "IsFullTypeNameSetExplicit")),
                                              Expression.Throw(Expression.New(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }), Expression.Constant("Changing the full type name for an ISerializable is not supported")))));
            expressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(si, "IsAssemblyNameSetExplicit")),
                                              Expression.Throw(Expression.New(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }), Expression.Constant("Changing the assembly name for an ISerializable is not supported")))));
            expressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop<string, object, SerializationInfoEnumerator>(typeof(SerializationInfo),
                                                                                                                     variables,
                                                                                                                     outputStream,
                                                                                                                     si,
                                                                                                                     objTracking,
                                                                                                                     GetLoopBodyCargo(outputStream, objTracking),
                                                                                                                     Expression.Property(si, typeof(SerializationInfo).GetProperty("MemberCount")),
                                                                                                                     enumeratorMethod,
                                                                                                                     loopBodyCargo));
            return Expression.Block(expressions);
        }

        internal static ConstructorInfo GetSerializationConstructor(Type type)
        {
            return type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new [] { typeof(SerializationInfo), typeof(StreamingContext) }, new ParameterModifier[0]);
        }

        private static Func<EnumerableLoopBodyCargo<string, object>, Expression> GetLoopBodyCargo(ParameterExpression outputStream, ParameterExpression objTracking)
        {
            Func<EnumerableLoopBodyCargo<string, object>, Expression> loopBody = cargo => {
                var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Name"));
                var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Value"));

                return Expression.Block(PrimitiveHelpers.WriteString(outputStream, keyExpression),
                                                       Serializer.GetWriteClassTypeExpression(outputStream, objTracking, valueExpression, cargo.ItemAsObj, cargo.TypeExpr, cargo.Serializer, typeof(object)));
            };

            

            return loopBody;
        }
    }
}
