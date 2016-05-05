//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using DesertOctopus.Utilities;

//namespace DesertOctopus.ObjectCloner
//{
//    public class ObjectCloner2
//    {
//        private static readonly ConcurrentDictionary<Type, Func<object, ObjectClonerReferenceTracker, object>> Cloners = new ConcurrentDictionary<Type, Func<object, ObjectClonerReferenceTracker, object>>();
//        private static readonly ConcurrentDictionary<Type, bool> TypeIsDictionary = new ConcurrentDictionary<Type, bool>();
//        private static readonly MethodInfo CloneImplMethodInfo = typeof(ObjectCloner).GetMethod("CloneImpl", BindingFlags.NonPublic | BindingFlags.Static);
//        private static readonly object SyncLock = new object();

//        public static T Clone<T>(T obj)
//        {
//            if (obj == null)
//            {
//                return default(T);
//            }

//            if (obj is Expression)
//            {
//                throw new NotSupportedException("Cannot clone expression.");
//            }

//            var refTracker = new ObjectClonerReferenceTracker();
//            return (T)CloneImpl(obj.GetType())(obj, refTracker);
//        }

//        private static Func<object, ObjectClonerReferenceTracker, object> CloneImpl(Type type)
//        {
//            Func<object, ObjectClonerReferenceTracker, object> cloner;
//            if (!Cloners.TryGetValue(type, out cloner))
//            {
//                lock (SyncLock)
//                {
//                    cloner = GenerateCloner(type);
//                    Cloners.TryAdd(type, cloner);
//                }
//            }
//            return cloner;
//        }

//        private static Func<object, ObjectClonerReferenceTracker, object> GenerateCloner(Type sourceType)
//        {
//            Expression resultExpression;
//            ParameterExpression sourceParameter = Expression.Parameter(typeof(object), "sourceParam");
//            ParameterExpression refTrackerParam = Expression.Parameter(typeof(ObjectClonerReferenceTracker), "refTrackerParam");

//            var clone = Expression.Parameter(sourceType, "newInstance");
//            var source = Expression.Parameter(sourceType, "source");
//            var returnTarget = Expression.Label(sourceType);
//            GotoExpression returnExpression;

//            var variables = new List<ParameterExpression>();
//            var expressions = new List<Expression>();
//            variables.Add(source);
//            expressions.Add(Expression.Assign(source,
//                                              Expression.Convert(sourceParameter, sourceType)));

//            if (sourceType.IsPrimitive || sourceType.IsValueType || (sourceType == typeof(string)))
//            {
//                // Primitives, value types and strings are copied on direct assignment
//                returnExpression = Expression.Return(returnTarget, source, sourceType);
//            }
//            /*
//            else if (typeof(ISerializable).IsAssignableFrom(type))
//            {
//                expressions.Add(ISerializableSerializer.GenerateISerializableExpression(type, variables, outputStream, objToSerialize, objCargo));
//            }
//            else if (type == typeof(ExpandoObject))
//            {
//                expressions.Add(ExpandoSerializer.GenerateExpandoObjectExpression(type, variables, outputStream, objToSerialize, objCargo));
//            }*/
//            else if (sourceType.IsArray)
//            {
//                variables.Add(clone);
//                GenerateCopyArrayExpression(source, clone, expressions, sourceType, refTrackerParam);
//                returnExpression = Expression.Return(returnTarget, clone, sourceType);
//            }
//            else
//            {
//                variables.Add(clone);
//                GenerateClassExpressions(sourceType, expressions, variables, source, clone, refTrackerParam);
//                returnExpression = Expression.Return(returnTarget, clone, sourceType);
//            }

//            expressions.Add(returnExpression);
//            var returnLabel = Expression.Label(returnTarget, Expression.Default(sourceType));
//            expressions.Add(returnLabel);

//            // Turn all transfer expressions into a single block if necessary
//            if ((expressions.Count == 1) && (variables.Count == 0))
//            {
//                resultExpression = expressions[0];
//            }
//            else
//            {
//                resultExpression = Expression.Block(variables, expressions);
//            }

//            // Value types require manual boxing
//            if (sourceType.IsValueType)
//            {
//                resultExpression = Expression.Convert(resultExpression, typeof(object));
//            }
//            return Expression.Lambda<Func<object, ObjectClonerReferenceTracker, object>>(resultExpression, sourceParameter, refTrackerParam).Compile();
//        }

//        private static void GenerateClassExpressions(Type sourceType,
//                                                     List<Expression> expressions,
//                                                     List<ParameterExpression> variables,
//                                                     ParameterExpression source,
//                                                     ParameterExpression clone,
//                                                     ParameterExpression refTrackerParam)
//        {
//             Expression expr;

//            if (sourceType == typeof(ExpandoObject))
//            {
//                expr = GenerateCopyExpandoObjectExpression(source, clone, variables, sourceType, refTrackerParam);
//            }
//            else if (IsGenericDictionary(sourceType))
//            {
//                expr = GenerateCopyDictionaryExpression(source, clone, variables, sourceType, refTrackerParam);
//            }
//            else
//            {
//                expr = GenerateCopyClassExpresssion(source, clone, sourceType, refTrackerParam);
//            }


//            expressions.Add(Expression.IfThenElse(Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("IsSourceObjectTracked"), source),
//                                                Expression.Assign(clone, Expression.Convert(Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("GetEquivalentTargetObject"), source), sourceType)),
                                                
//                                                expr)
//                            );
//        }

//        private static Expression GenerateCopyDictionaryExpression(ParameterExpression source,
//                                                                   ParameterExpression clone,
//                                                                   List<ParameterExpression> variables,
//                                                                   Type sourceType,
//                                                                   ParameterExpression refTrackerParam)
//        {
//            // The internal constructor of a dictionary can sometime contains circular reference.
//            // Instead of using GetUninitializedObject, this method does the following:
//            //  - uses the public parameterless constructor to create the object
//            //  - copy the fields of the types that are not defined on the Dictionary or ConcurrentDictionary class
//            //  - call the Add method to add items to it

//            var baseDictType = GetDictionaryType(sourceType);
//            if (baseDictType == null)
//            {
//                throw new InvalidOperationException("The dictionary must inherits from Dictionary or ConcurrentDictionary. " + sourceType);
//            }

//            if (sourceType.GetConstructor(new Type[0]) == null)
//            {
//                throw new MissingMethodException("Type " + sourceType + " does not have a default constructor.");
//            }

//            var allFields = InternalSerializationStuff.GetFields(sourceType).ToArray();
//            var allBaseDictFields = InternalSerializationStuff.GetFields(baseDictType).ToArray();
//            var baseDictFields = allBaseDictFields
//                                    .Where(x => !(x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(IEqualityComparer<>)))
//                                    .ToArray();
//            var filteredFields = allFields.Except(baseDictFields).ToArray(); // do not copy the fields of the base dictionary except for the comparer

//            var expressions = new List<Expression>();

//            var execptionCtor = typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });
//            if (execptionCtor == null)
//            {
//                throw new MissingMethodException("Type " + typeof(InvalidOperationException) + " does not have a default constructor.");
//            }

//            expressions.Add(Expression.IfThen(Expression.Equal(Expression.Call(typeof(ObjectCloner).GetMethod("GetDictionaryType", BindingFlags.Static | BindingFlags.NonPublic), Expression.Constant(sourceType)), Expression.Constant(null)),
//                                              Expression.Throw(Expression.New(execptionCtor, Expression.Constant("The dictionary must inherits from Dictionary or ConcurrentDictionary. " + sourceType)))));

//            expressions.Add(Expression.Assign(clone, Expression.Convert(Expression.Convert(Expression.New(sourceType), typeof(object)), sourceType)));
//            expressions.Add(Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("Track"), source, clone));

//            GenerateCopyFieldsExpressions(filteredFields, source, clone, expressions, refTrackerParam);

//            var genericMethod = typeof(ObjectCloner).GetMethod("CopyDictionaryValues", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(baseDictType.GetGenericArguments());

//            expressions.Add((Expression)genericMethod.Invoke(null, new object[] { source, clone, variables, refTrackerParam }));

//            return Expression.Block(expressions);
//        }

//        private static Type GetDictionaryType(Type type)
//        {
//            var targetType = type;
//            do
//            {
//                if (targetType.IsGenericType
//                    && (targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>) || targetType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>)))
//                {
//                    return targetType;
//                }

//                targetType = targetType.BaseType;
//            } while (targetType != null);

//            return null;
//        }

//        private static Expression GenerateCopyClassExpresssion(ParameterExpression source,
//                                                               ParameterExpression clone,
//                                                               Type sourceType,
//                                                               ParameterExpression refTrackerParam)
//        {
//            var ctor = Expression.Call(CreateUninitializedMethodInfo.GetMethodInfo(), Expression.Constant(sourceType));

//            var expressions = new List<Expression>();
//            var fields = InternalSerializationStuff.GetFields(sourceType);
//            expressions.Add(Expression.Assign(clone, Expression.Convert(ctor, sourceType)));
//            expressions.Add(Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("Track"), source, clone));
//            GenerateCopyFieldsExpressions(fields, source, clone, expressions, refTrackerParam);

//            return Expression.Block(expressions);
//        }

//        private static void GenerateCopyFieldsExpressions(IEnumerable<FieldInfo> fields,
//                                                          ParameterExpression source,
//                                                          ParameterExpression clone,
//                                                          List<Expression> expressions,
//                                                          ParameterExpression refTrackerParam)
//        {
//            foreach (var field in fields)
//            {
//                if (field.FieldType.IsPrimitive || field.FieldType.IsValueType || (field.FieldType == typeof(string)))
//                {
//                    if (field.IsInitOnly)
//                    {
//                        expressions.Add(Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
//                                                                Expression.Constant(field),
//                                                                Expression.Convert(Expression.Field(source, field), typeof(object)),
//                                                                clone));
//                    }
//                    else
//                    {
//                        var from = Expression.Field(source, field);
//                        var to = Expression.Field(clone, field);
//                        expressions.Add(Expression.Assign(to, from));
//                    }
//                }
//                else
//                {
//                    if (field.IsInitOnly)
//                    {
//                        expressions.Add(Expression.IfThen(Expression.NotEqual(Expression.Field(source, field), Expression.Constant(null)),
//                                                          Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
//                                                                          Expression.Constant(field),
//                                                                          Expression.Convert(CallCopyExpression(Expression.Field(source, field), refTrackerParam), typeof(object)),
//                                                                          clone)));
//                    }
//                    else
//                    {
//                        var a = CallCopyExpression(Expression.Field(source, field), refTrackerParam);
//                        var b = Expression.Assign(Expression.Field(clone, field),
//                                                  Expression.Convert(a, field.FieldType));
//                        expressions.Add(Expression.IfThen(Expression.NotEqual(Expression.Field(source, field), Expression.Constant(null)),
//                                                          b));
//                    }
//                }
//            }
//        }

//        private static Expression GenerateCopyExpandoObjectExpression(ParameterExpression source,
//                                                                      ParameterExpression clone,
//                                                                      List<ParameterExpression> variables,
//                                                                      Type sourceType,
//                                                                      ParameterExpression refTrackerParam)
//        {
//            return Expression.Block(Expression.Assign(clone, Expression.Convert(Expression.Convert(Expression.New(sourceType), typeof(object)), sourceType)),
//                                    Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("Track"), source, clone),
//                                    CopyDictionaryValues<string, object>(source, clone, variables, refTrackerParam));
//        }

//        private static Expression CopyDictionaryValues<TKey, TValue>(ParameterExpression source,
//                                                                     ParameterExpression clone,
//                                                                     List<ParameterExpression> variables,
//                                                                     ParameterExpression refTrackerParam)
//        {
//            // Equivalent of the following code:

//            //IDictionary<string, Object> s = null;
//            //IDictionary<string, Object> d = null;
//            //IEnumerator<KeyValuePair<string, object>> enumerator = s.GetEnumerator();
//            //try
//            //{
//            //    while (enumerator.MoveNext())
//            //    {
//            //        var key = enumerator.Current.Key;
//            //        var value = enumerator.Current.Value;
//            //        if (value != null)
//            //            d.Add(key, clone(value));
//            //        else
//            //            d.Add(key, null);
//            //    }
//            //}
//            //finally
//            //{
//            //    enumerator.Dispose();
//            //}


//            var sourceDict = Expression.Parameter(typeof(IDictionary<TKey, TValue>), "sourceDict");
//            variables.Add(sourceDict);
//            var cloneDict = Expression.Parameter(typeof(IDictionary<TKey, TValue>), "cloneDict");
//            variables.Add(cloneDict);

//            var kvpType = typeof(KeyValuePair<TKey, TValue>);
//            var enumeratorType = typeof(IEnumerator<KeyValuePair<TKey, TValue>>);
//            var enumerableType = typeof(IEnumerable<KeyValuePair<TKey, TValue>>);
//            var idictType = typeof(IDictionary<TKey, TValue>);
//            var breakLabel2 = Expression.Label("breakLabel");

//            var enumeratorVar = Expression.Parameter(enumeratorType, "enumeratorVar");
//            variables.Add(enumeratorVar);

//            var keyExpression = Expression.Property(Expression.Property(enumeratorVar, enumeratorType.GetProperty("Current")), kvpType.GetProperty("Key"));
//            var valueExpression = Expression.Property(Expression.Property(enumeratorVar, enumeratorType.GetProperty("Current")), kvpType.GetProperty("Value"));

//            Expression loopBody;
//            if ((typeof(TValue).IsPrimitive || typeof(TValue).IsValueType) && !typeof(TValue).IsGenericType) // exclude nullable types
//            {
//                loopBody = Expression.Call(cloneDict, idictType.GetMethod("Add"), keyExpression, Expression.Convert(CallCopyExpression(Expression.Convert(valueExpression, typeof(object)), refTrackerParam), typeof(TValue)));
//            }
//            else
//            {
//                var addExpr = Expression.Call(cloneDict, idictType.GetMethod("Add"), keyExpression, Expression.Convert(CallCopyExpression(valueExpression, refTrackerParam), typeof(TValue)));
//                var addNullExpr = Expression.Call(cloneDict, idictType.GetMethod("Add"), keyExpression, Expression.Convert(Expression.Constant(null), typeof(TValue)));
//                loopBody = Expression.IfThenElse(Expression.NotEqual(valueExpression, Expression.Constant(null, typeof(TValue))), addExpr, addNullExpr);
//            }

//            return Expression.TryFinally(Expression.Block(Expression.Assign(sourceDict, Expression.Convert(source, idictType)),
//                                                          Expression.Assign(cloneDict, Expression.Convert(clone, idictType)),
//                                                          Expression.Assign(enumeratorVar, Expression.Call(sourceDict, enumerableType.GetMethod("GetEnumerator"))),
//                                                          Expression.Loop(Expression.IfThenElse(Expression.IsTrue(Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"))),
//                                                                                                loopBody,
//                                                                                                Expression.Break(breakLabel2)),
//                                                                                   breakLabel2)),
//                                        Expression.IfThen(Expression.NotEqual(enumeratorVar, Expression.Constant(null)),
//                                                          Expression.Call(enumeratorVar, typeof(IDisposable).GetMethod("Dispose"))));
//        }

//        private static void GenerateCopyArrayExpression(ParameterExpression original,
//                                                              ParameterExpression clone,
//                                                              List<Expression> expressions,
//                                                              Type clonedType,
//                                                              ParameterExpression refTrackerParam)
//        {
//            var elementType = clonedType.GetElementType();
//            if (elementType.IsPrimitive || elementType.IsValueType || (elementType == typeof(string)))
//            {
//                var copyMethod = ArrayMIH.Clone();
//                var assignExprPrimitive = Expression.Assign(clone, Expression.Convert(Expression.Call(Expression.Convert(original, typeof(Array)), copyMethod), clonedType));
//                expressions.Add(assignExprPrimitive);
//                return;
//            }

//            var arrayLengthExpr = Expression.Call(original, CreateArrayMethodInfo.GetArrayLengthMethod(), Expression.Constant(0));
//            var assignExpr = Expression.Assign(clone, Expression.Convert(Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(elementType), arrayLengthExpr), clonedType));
//            expressions.Add(assignExpr);

//            var index = Expression.Parameter(typeof(int));
//            var item = Expression.Parameter(elementType);
//            var breakLabel = Expression.Label("breakLabel");

//            var loopBody = Expression.Block(new[] { item },
//                                            Expression.Assign(item, Expression.ArrayAccess(original, index)),
//                                            Expression.IfThen(Expression.NotEqual(item, Expression.Constant(null)),
//                                                                Expression.Assign(Expression.ArrayAccess(clone, index),
//                                                                                  Expression.Convert(CallCopyExpression(item, refTrackerParam), elementType))));

//            var loop = Expression.Block(new[] { index },
//                                        Expression.Assign(index, Expression.Constant(0)),
//                                        Expression.Loop(Expression.IfThenElse(Expression.LessThan(index, arrayLengthExpr),
//                                                                              Expression.Block(loopBody,
//                                                                                               Expression.AddAssign(index, Expression.Constant(1))),
//                                                                              Expression.Break(breakLabel)),
//                                                        breakLabel));
//            expressions.Add(loop);
//        }

//        private static Expression CallCopyExpression(Expression item, ParameterExpression refTrackerParam)
//        {
//            MethodInfo invokeMethodInfo = typeof(Func<object, ObjectClonerReferenceTracker, object>).GetMethod("Invoke");

//            var typeExpr = Expression.Call(item, ObjectMIH.GetTypeMethod());
//            var generateTypeExpr = Expression.Call(CloneImplMethodInfo, typeExpr);

//            return Expression.Call(generateTypeExpr, invokeMethodInfo, Expression.Convert(item, typeof(object)), refTrackerParam);
//        }

//        private static bool IsGenericDictionary(Type sourceType)
//        {
//            return TypeIsDictionary.GetOrAdd(sourceType,
//                                             t => t.GetInterfaces().Any(x => x.IsGenericType && x.Name == "IDictionary`2"));
//        }

//        private static class CreateUninitializedMethodInfo
//        {
//            private static readonly MethodInfo Method = typeof(System.Runtime.Serialization.FormatterServices).GetMethod("GetUninitializedObject", BindingFlags.Public | BindingFlags.Static);

//            public static MethodInfo GetMethodInfo()
//            {
//                return Method;
//            }
//        }

//        private static class CopyReadOnlyFieldMethodInfo
//        {
//            private static readonly MethodInfo Method = typeof(CopyReadOnlyFieldMethodInfo).GetMethod("CopyReadonlyField", BindingFlags.NonPublic | BindingFlags.Static);

//            public static MethodInfo GetMethodInfo()
//            {
//                return Method;
//            }

//            private static void CopyReadonlyField(FieldInfo field, object value, object target)
//            {
//                // using reflection to copy readonly fields.  It's slower but it's the only choice
//                field.SetValue(target, value);
//            }
//        }

//        private static class CreateArrayMethodInfo
//        {
//            private static readonly MethodInfo CreateArrayMethod = typeof(CreateArrayMethodInfo).GetMethod("CreateArray", BindingFlags.Static | BindingFlags.Public);
//            private static readonly MethodInfo ArrayLengthMethod = typeof(Array).GetMethod("GetLength");

//            public static MethodInfo GetCreateArrayMethodInfo(Type elementType)
//            {
//                return CreateArrayMethod.MakeGenericMethod(elementType);
//            }

//            public static MethodInfo GetArrayLengthMethod()
//            {
//                return ArrayLengthMethod;
//            }

//            public static T[] CreateArray<T>(int length)
//            {
//                return new T[length];
//            }
//        }


//    }
//}
