using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using DesertOctopus.Exceptions;
using DesertOctopus.ObjectCloner;
using DesertOctopus.Serialization;
using DesertOctopus.Utilities;

namespace DesertOctopus.Cloning
{
    internal static class ISerializableCloner
    {
        public static Expression GenerateISerializableExpression(List<ParameterExpression> variables,
                                                                 ParameterExpression source,
                                                                 ParameterExpression clone,
                                                                 Type sourceType,
                                                                 ParameterExpression refTrackerParam)
        {
            var serializationConstructor = ISerializableSerializer.GetSerializationConstructor(sourceType);
            if (serializationConstructor == null)
            {
                throw new MissingConstructorException("Cannot serialize type " + sourceType + " because it does not have the required constructor for ISerializable.  If you inherits from a class that implements ISerializable you have to expose the serialization constructor.");
            }
            var getEnumeratorMethodInfo = SerializationInfoMIH.GetEnumerator();
            if (getEnumeratorMethodInfo == null)
            {
                throw new Exception("Could not find GetEnumerator method.");
            }

            var siSource = Expression.Parameter(typeof(SerializationInfo), "siSource");
            var fc = Expression.Parameter(typeof(FormatterConverter), "fc");
            var context = Expression.Parameter(typeof(StreamingContext), "context");
            var iserSource = Expression.Parameter(typeof(ISerializable), "iserSource");
            var siClone = Expression.Parameter(typeof(SerializationInfo), "siClone");
            var cloner = Expression.Parameter(typeof(Func<object, ObjectClonerReferenceTracker, object>), "cloner");
            var clonedItem = Expression.Parameter(typeof(object), "clonedItem");

            variables.Add(fc);
            variables.Add(context);
            variables.Add(siSource);
            variables.Add(iserSource);
            variables.Add(siClone);
            variables.Add(cloner);
            variables.Add(clonedItem);

            var enumeratorMethod = Expression.Call(siSource,
                                                   getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo<string, object>();
            loopBodyCargo.EnumeratorType = typeof(SerializationInfoEnumerator);
            loopBodyCargo.KvpType = typeof(SerializationEntry);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(fc, Expression.New(typeof(FormatterConverter))));
            expressions.Add(Expression.Assign(context, Expression.New(StreamingContextMIH.Constructor(), Expression.Constant(StreamingContextStates.All))));
            expressions.Add(Expression.Assign(siSource, Expression.New(SerializationInfoMIH.Constructor(), Expression.Constant(sourceType), fc)));
            expressions.Add(Expression.Assign(iserSource, Expression.Convert(source, typeof(ISerializable))));
            expressions.Add(Expression.Assign(siClone, Expression.New(SerializationInfoMIH.Constructor(), Expression.Constant(sourceType), fc)));

            expressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializingAttributeExpression(sourceType, source, context));
            expressions.Add(Expression.Call(iserSource, ISerializableMIH.GetObjectData(), siSource, context));
            expressions.AddRange(SerializationCallbacksHelper.GenerateOnSerializedAttributeExpression(sourceType, source, context));

            expressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(siSource, "IsFullTypeNameSetExplicit")),
                                              Expression.Throw(Expression.New(InvalidOperationExceptionMIH.Constructor(),
                                                                              Expression.Constant("Changing the full type name for an ISerializable is not supported")))));
            expressions.Add(Expression.IfThen(Expression.IsTrue(Expression.Property(siSource, "IsAssemblyNameSetExplicit")),
                                              Expression.Throw(Expression.New(InvalidOperationExceptionMIH.Constructor(),
                                                                              Expression.Constant("Changing the assembly name for an ISerializable is not supported")))));

            expressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop<string, object, SerializationInfoEnumerator>(variables,
                                                                                                                     GetLoopBodyCargo(siClone, cloner, clonedItem, refTrackerParam),
                                                                                                                     enumeratorMethod,
                                                                                                                     null,
                                                                                                                     loopBodyCargo));

            expressions.Add(Expression.Assign(clone, Expression.New(serializationConstructor, siClone, context)));
            expressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializedAttributeExpression(sourceType, clone, context));
            expressions.Add(SerializationCallbacksHelper.GenerateCallIDeserializationExpression(sourceType, clone));

            return Expression.Block(expressions);
        }

        private static Func<EnumerableLoopBodyCargo<string, object>, Expression> GetLoopBodyCargo(ParameterExpression siClone, ParameterExpression cloner, ParameterExpression clonedItem, ParameterExpression refTrackerParam)
        {
            Func<EnumerableLoopBodyCargo<string, object>, Expression> loopBody = cargo => {
                                                                                              var keyExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Name"));
                                                                                              var valueExpression = Expression.Property(Expression.Property(cargo.Enumerator, cargo.EnumeratorType.GetProperty("Current")), cargo.KvpType.GetProperty("Value"));
                                                                                             
                                                                                              var cloneValueExpr = Expression.Block(ClassCloner.GetCloneClassTypeExpression(refTrackerParam, valueExpression, clonedItem, typeof(object), cloner),
                                                                                                                                    Expression.Call(siClone, SerializationInfoMIH.AddValue(), keyExpression, clonedItem));

                                                                                              return Expression.IfThenElse(Expression.Equal(valueExpression, Expression.Constant(null)),
                                                                                                                           Expression.Call(siClone, SerializationInfoMIH.AddValue(), keyExpression, Expression.Constant((object)null, typeof(object))),
                                                                                                                           cloneValueExpr);
                                                                                            };



            return loopBody;
        }
    }
}