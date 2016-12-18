using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Exceptions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    internal class DictionaryDeserializer
    {
        public static Expression GenerateDictionaryGenericExpression(Type type,
                                                                     List<ParameterExpression> variables,
                                                                     ParameterExpression inputStream,
                                                                     ParameterExpression objTracker)
        {
            var ctor = type.GetConstructor(new Type[0]);
            if (ctor == null)
            {
                throw new MissingConstructorException("Type " + type + " must have a public constructor without parameter.");
            }

            var notTrackedExpressions = new List<Expression>();
            var newInstance = Expression.Parameter(type, "newInstanceDict");
            var comparerType = Expression.Parameter(typeof(byte), "comparerType");

            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "typeExpr");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");
            var deserializer = Expression.Parameter(typeof(Func<Stream, DeserializerObjectTracker, object>), "deserializer");

            variables.Add(deserializer);
            variables.Add(typeExpr);
            variables.Add(typeName);
            variables.Add(typeHashCode);


            variables.Add(newInstance);
            variables.Add(comparerType);

            var genericDictionaryType = DictionaryHelper.GetDictionaryType(type);
            var comparerConstructor = DictionaryHelper.GetComparerConstructor(type);

            var deserializeDictionaryWithDefaultComparer = DeserializeDictionary(type,
                                                                                 genericDictionaryType,
                                                                                 variables,
                                                                                 newInstance,
                                                                                 inputStream,
                                                                                 objTracker,
                                                                                 deserializer,
                                                                                 typeExpr,
                                                                                 typeName,
                                                                                 typeHashCode,
                                                                                 (exprs, newIns) => exprs.Add(Expression.Assign(newIns, Expression.New(type.GetConstructor(new Type[0])))));

            if (comparerConstructor == null)
            {
                notTrackedExpressions.Add(PrimitiveHelpers.ReadByte(inputStream, objTracker));
                notTrackedExpressions.Add(deserializeDictionaryWithDefaultComparer);
            }
            else
            {
                notTrackedExpressions.Add(Expression.IfThenElse(Expression.Equal(PrimitiveHelpers.ReadByte(inputStream, objTracker), Expression.Constant(0, typeof(int))),
                                                                deserializeDictionaryWithDefaultComparer,
                                                                DeserializeDictionary(type,
                                                                                      genericDictionaryType,
                                                                                      variables,
                                                                                      newInstance,
                                                                                      inputStream,
                                                                                      objTracker,
                                                                                      deserializer,
                                                                                      typeExpr,
                                                                                      typeName,
                                                                                      typeHashCode,
                                                                                      (exprs, newIns) =>
                                                                                      {
                                                                                          var eqType = typeof(IEqualityComparer<>).MakeGenericType(genericDictionaryType.GetGenericArguments()[0]);
                                                                                          var comparer = Expression.Parameter(eqType, "comparer");
                                                                                          variables.Add(comparer);
                                                                                          exprs.Add(Deserializer.GetReadClassExpression(inputStream,
                                                                                                                                        objTracker,
                                                                                                                                        comparer,
                                                                                                                                        typeExpr,
                                                                                                                                        typeName,
                                                                                                                                        typeHashCode,
                                                                                                                                        deserializer,
                                                                                                                                        eqType));
                                                                                          exprs.Add(Expression.Assign(newIns, Expression.New(comparerConstructor, comparer)));
                                                                                      })));
            }



            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracker,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         comparerType,
                                                                         variables);
        }

        private static Expression DeserializeDictionary(Type type,
                                                        Type genericDictionaryType,
                                                        List<ParameterExpression> variables,
                                                        ParameterExpression newInstance,
                                                        ParameterExpression inputStream,
                                                        ParameterExpression objTracker,
                                                        ParameterExpression deserializer,
                                                        ParameterExpression typeExpr,
                                                        ParameterExpression typeName,
                                                        ParameterExpression typeHashCode,
                                                        Action<List<Expression>, ParameterExpression> createClassAction)
        {
            var expressions = new List<Expression>();

            var trackType = Expression.Parameter(typeof(byte), "isAlreadyTracked");
            variables.Add(trackType);
            var temporaryVariables = new Dictionary<Type, ParameterExpression>();

            var fields = InternalSerializationStuff.GetFields(type, genericDictionaryType);

            createClassAction(expressions, newInstance);
            expressions.Add(Expression.Call(objTracker, DeserializerObjectTrackerMih.TrackedObject(), newInstance));

            Deserializer.GenerateReadFieldsExpression(type,
                                                      fields,
                                                      inputStream,
                                                      objTracker,
                                                      temporaryVariables,
                                                      variables,
                                                      newInstance,
                                                      expressions,
                                                      typeExpr,
                                                      typeName,
                                                      typeHashCode,
                                                      deserializer);

            expressions.AddRange(DeserializeDictionaryValues(type, variables, inputStream, objTracker, newInstance));

            //expressions.AddRange(SerializationCallbacksHelper.GenerateOnDeserializedAttributeExpression(type, newInstance, Expression.New(StreamingContextMih.Constructor(), Expression.Constant(StreamingContextStates.All))));
            //expressions.Add(SerializationCallbacksHelper.GenerateCallIDeserializationExpression(type, newInstance));

            return Expression.Block(expressions);
        }

        internal static List<Expression> DeserializeDictionaryValues(Type type,
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

            notTrackedExpressions.Add(loop);
            notTrackedExpressions.Add(newInstance);

            return notTrackedExpressions;
        }

        internal static void GetReadExpression(ParameterExpression inputStream,
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
