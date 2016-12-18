using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Exceptions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    internal class DictionarySerializer
    {
        public static Expression GenerateDictionaryGenericExpression(Type type,
                                                                     List<ParameterExpression> variables,
                                                                     ParameterExpression outputStream,
                                                                     ParameterExpression objToSerialize,
                                                                     ParameterExpression objTracking)
        {
            var ctor = type.GetConstructor(new Type[0]);
            if (ctor == null)
            {
                throw new MissingConstructorException("Type " + type + " must have a public constructor without parameter.");
            }

            var notTrackedExpressions = new List<Expression>();

            var genericDictionaryType = DictionaryHelper.GetDictionaryType(type);
            var comparerConstructor = DictionaryHelper.GetComparerConstructor(type);
            var serializeDictionaryWithDefaultComparer = SerializeDictionaryWithDefaultComparer(genericDictionaryType,
                                                                                                variables,
                                                                                                type,
                                                                                                outputStream,
                                                                                                objToSerialize,
                                                                                                objTracking);
            if (comparerConstructor == null)
            {
                notTrackedExpressions.Add(serializeDictionaryWithDefaultComparer);
            }
            else
            {
                notTrackedExpressions.Add(Expression.IfThenElse(Expression.IsTrue(Expression.Call(DictionaryMih.IsDefaultEqualityComparer(),
                                                                                                  Expression.Constant(genericDictionaryType.GetGenericArguments()[0]),
                                                                                                  Expression.Property(objToSerialize, nameof(Dictionary<int, int>.Comparer)))),
                                                                serializeDictionaryWithDefaultComparer,
                                                                SerializeDictionaryWithCustomComparer(genericDictionaryType,
                                                                                                      variables,
                                                                                                      type,
                                                                                                      outputStream,
                                                                                                      objToSerialize,
                                                                                                      objTracking)));
            }

            return Serializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                       outputStream,
                                                                       objToSerialize,
                                                                       objTracking,
                                                                       notTrackedExpressions,
                                                                       variables);
        }

        private static Expression SerializeDictionaryWithDefaultComparer(Type genericDictionaryType,
                                                                         List<ParameterExpression> variables,
                                                                         Type type,
                                                                         ParameterExpression outputStream,
                                                                         ParameterExpression objToSerialize,
                                                                         ParameterExpression objTracking)
        {
            List<Expression> notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value0, typeof(byte)), objTracking));

            var fields = InternalSerializationStuff.GetFields(type, genericDictionaryType);

            Serializer.GenerateFieldsExpression(type,
                                                fields,
                                                outputStream,
                                                objToSerialize,
                                                objTracking,
                                                variables,
                                                notTrackedExpressions);

            notTrackedExpressions.Add(SerializeDictionary(type,
                                                          genericDictionaryType,
                                                          variables,
                                                          outputStream,
                                                          objToSerialize,
                                                          objTracking,
                                                          false));

            return Expression.Block(notTrackedExpressions);
        }

        private static Expression SerializeDictionaryWithCustomComparer(Type genericDictionaryType,
                                                                        List<ParameterExpression> variables,
                                                                        Type type,
                                                                        ParameterExpression outputStream,
                                                                        ParameterExpression objToSerialize,
                                                                        ParameterExpression objTracking)
        {
            var serializer = Expression.Parameter(typeof(Action<Stream, object, SerializerObjectTracker>), "serializer");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");

            variables.Add(serializer);
            variables.Add(typeExpr);
            variables.Add(itemAsObj);

            List<Expression> notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1, typeof(byte)), objTracking));

            var comparerProperty = Expression.Property(objToSerialize, nameof(Dictionary<int, int>.Comparer));
            notTrackedExpressions.Add(Serializer.GetWriteClassTypeExpression(outputStream,
                                                                             objTracking,
                                                                             comparerProperty,
                                                                             itemAsObj,
                                                                             typeExpr,
                                                                             serializer,
                                                                             typeof(object)));

            var fields = InternalSerializationStuff.GetFields(type, genericDictionaryType);

            Serializer.GenerateFieldsExpression(type,
                                                fields,
                                                outputStream,
                                                objToSerialize,
                                                objTracking,
                                                variables,
                                                notTrackedExpressions);

            notTrackedExpressions.Add(SerializeDictionary(type,
                                                          genericDictionaryType,
                                                          variables,
                                                          outputStream,
                                                          objToSerialize,
                                                          objTracking,
                                                          false));

            return Expression.Block(notTrackedExpressions);
        }

        internal static Expression SerializeDictionary(Type type,
                                                       Type dictionaryType,
                                                       List<ParameterExpression> variables,
                                                       ParameterExpression outputStream,
                                                       ParameterExpression objToSerialize,
                                                       ParameterExpression objTracking,
                                                       bool addTracking)
        {
            MethodInfo getEnumeratorMethodInfo = type.GetMethod("GetEnumerator");
            var enumeratorMethod = Expression.Call(Expression.Convert(objToSerialize, type), getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo();
            loopBodyCargo.EnumeratorType = getEnumeratorMethodInfo.ReturnType;
            loopBodyCargo.KvpType = typeof(KeyValuePair<,>).MakeGenericType(dictionaryType.GetGenericArguments());

            var notTrackedExpressions = new List<Expression>();
            if (addTracking)
            {
                notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));
                notTrackedExpressions.Add(PrimitiveHelpers.WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1), objTracking));
            }

            notTrackedExpressions.Add(PrimitiveHelpers.WriteInt32(outputStream, Expression.Property(Expression.Convert(objToSerialize, dictionaryType), "Count"), objTracking));

            notTrackedExpressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop(variables,
                                                                                  GetKeyValuePairLoopBodyCargo(outputStream, objTracking, "Key", dictionaryType.GetGenericArguments()[0], "Value", dictionaryType.GetGenericArguments()[1]),
                                                                                  enumeratorMethod,
                                                                                  null,
                                                                                  loopBodyCargo));

            return Expression.Block(notTrackedExpressions);
        }

        internal static Func<EnumerableLoopBodyCargo, Expression> GetKeyValuePairLoopBodyCargo(ParameterExpression outputStream,
                                                                                               ParameterExpression objTracking,
                                                                                               string keyPropertyName,
                                                                                               Type keyType,
                                                                                               string valuePropertyname,
                                                                                               Type valueType)
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
                var primitiveSerializer = Serializer.GetTypeSerializer(valueType);
                return Expression.Invoke(Expression.Constant(primitiveSerializer), outputStream, valueExpression, objTracking);
            }

            return Serializer.GetWriteClassTypeExpression(outputStream, objTracking, valueExpression, cargo.ItemAsObj, cargo.TypeExpr, cargo.Serializer, valueType);
        }
    }
}
