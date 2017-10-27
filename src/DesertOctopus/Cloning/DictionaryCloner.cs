using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Exceptions;
using DesertOctopus.Utilities;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    internal class DictionaryCloner
    {
        public static Expression GenerateDictionaryGenericExpression(List<ParameterExpression> variables,
                                                                     ParameterExpression source,
                                                                     ParameterExpression clone,
                                                                     Type sourceType,
                                                                     ParameterExpression refTrackerParam)
        {
            var ctor = ReflectionHelpers.GetPublicConstructor(sourceType);
            if (ctor == null)
            {
                throw new MissingConstructorException("Type " + sourceType + " must have a public constructor without parameter.");
            }

            var genericDictionaryType = DictionaryHelper.GetDictionaryType(sourceType);
            var comparerConstructor = DictionaryHelper.GetComparerConstructor(sourceType);
            var cloneDictionaryWithDefaultComparer = CloneDictionaryWithDefaultComparer(genericDictionaryType,
                                                                                        variables,
                                                                                        source,
                                                                                        clone,
                                                                                        sourceType,
                                                                                        refTrackerParam);

            if (comparerConstructor == null)
            {
                return cloneDictionaryWithDefaultComparer;
            }

            return Expression.IfThenElse(Expression.IsTrue(Expression.Call(DictionaryMih.IsDefaultEqualityComparer(),
                                                                           Expression.Constant(genericDictionaryType.GetTypeInfo().GetGenericArguments()[0]),
                                                                           Expression.Property(source, nameof(Dictionary<int, int>.Comparer)))),
                                         cloneDictionaryWithDefaultComparer,
                                         CloneDictionaryWithCustomComparer(genericDictionaryType,
                                                                           comparerConstructor,
                                                                           variables,
                                                                           source,
                                                                           clone,
                                                                           sourceType,
                                                                           refTrackerParam));
        }

        private static Expression CloneDictionaryWithDefaultComparer(Type genericDictionaryType,
                                                                     List<ParameterExpression> variables,
                                                                     ParameterExpression source,
                                                                     ParameterExpression clone,
                                                                     Type sourceType,
                                                                     ParameterExpression refTrackerParam)
        {
            var ctor = ReflectionHelpers.GetPublicConstructor(sourceType);
            if (ctor == null)
            {
                throw new MissingConstructorException("Type " + sourceType + " must have a public constructor without parameter.");
            }

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(clone, Expression.New(ctor)));
            expressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));

            CopyFieldsAndValues(genericDictionaryType,
                       source,
                       clone,
                       sourceType,
                       refTrackerParam,
                       expressions,
                       variables);

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(expressions));
        }

        private static Expression CloneDictionaryWithCustomComparer(Type genericDictionaryType,
                                                                    ConstructorInfo comparerConstructor,
                                                                    List<ParameterExpression> variables,
                                                                    ParameterExpression source,
                                                                    ParameterExpression clone,
                                                                    Type sourceType,
                                                                    ParameterExpression refTrackerParam)
        {
            if (comparerConstructor == null)
            {
                throw new MissingConstructorException("Type " + sourceType + " must have a public constructor that takes an IEqualityComparer<> parameter.");
            }

            var expressions = new List<Expression>();
            var sourceComparer = Expression.Property(source, nameof(Dictionary<int, int>.Comparer));
            var comparerType = typeof(IEqualityComparer<>).MakeGenericType(genericDictionaryType.GetTypeInfo().GetGenericArguments()[0]);
            var clonedComparer = ClassCloner.CallCopyExpression(sourceComparer,
                                                                refTrackerParam,
                                                                Expression.Constant(comparerType));

            expressions.Add(Expression.Assign(clone, Expression.New(comparerConstructor, Expression.Convert(clonedComparer, comparerType))));
            expressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));

            CopyFieldsAndValues(genericDictionaryType,
                       source,
                       clone,
                       sourceType,
                       refTrackerParam,
                       expressions,
                       variables);

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(expressions));
        }

        private static void CopyFieldsAndValues(Type genericDictionaryType,
                                                ParameterExpression source,
                                                ParameterExpression clone,
                                                Type sourceType,
                                                ParameterExpression refTrackerParam,
                                                List<Expression> expressions,
                                                List<ParameterExpression> variables)
        {
            var clonedItem = Expression.Parameter(typeof(object), "clonedItem");
            variables.Add(clonedItem);

            var fields = InternalSerializationStuff.GetFields(sourceType,
                                                              genericDictionaryType);
            ClassCloner.GenerateCopyFieldsExpressions(fields,
                                                      source,
                                                      clone,
                                                      expressions,
                                                      refTrackerParam,
                                                      clonedItem);

            var typeKey = genericDictionaryType.GenericTypeArguments[0];
            var typeValue = genericDictionaryType.GenericTypeArguments[1];

            if (IsPrimitive(typeKey)
                && IsPrimitive(typeValue))
            {
                expressions.Add(Expression.Call(typeof(DictionaryCloner),
                                                nameof(DictionaryCloner.CopyPrimitiveKeyAndPrimitiveValue),
                                                genericDictionaryType.GenericTypeArguments,
                                                source,
                                                clone));
            }
            else if (!IsPrimitive(typeKey)
                     && IsPrimitive(typeValue))
            {
                expressions.Add(Expression.Call(typeof(DictionaryCloner),
                                                nameof(DictionaryCloner.CopyObjectKeyAndPrimitiveValue),
                                                genericDictionaryType.GenericTypeArguments,
                                                source,
                                                clone,
                                                refTrackerParam));
            }
            else if (IsPrimitive(typeKey)
                     && !IsPrimitive(typeValue))
            {
                expressions.Add(Expression.Call(typeof(DictionaryCloner),
                                                nameof(DictionaryCloner.CopyPrivitiveKeyAndObjectValue),
                                                genericDictionaryType.GenericTypeArguments,
                                                source,
                                                clone,
                                                refTrackerParam));
            }
            else
            {
                expressions.Add(Expression.Call(typeof(DictionaryCloner),
                                                nameof(DictionaryCloner.CopyObjectKeyAndObjectValue),
                                                genericDictionaryType.GenericTypeArguments,
                                                source,
                                                clone,
                                                refTrackerParam));
            }
        }

        private static bool IsPrimitive(Type type)
        {
            return type.GetTypeInfo().IsPrimitive
                   || type.GetTypeInfo().IsValueType
                   || (type == typeof(string));
        }

        private static T CloneValueAndTrack<T>(T value, ObjectClonerReferenceTracker refTracker)
            where T : class
        {
            var trackedValue = refTracker.GetEquivalentTargetObject(value) as T;
            if (trackedValue == null)
            {
                var clonedValue = ObjectCloner.Clone(value);
                refTracker.Track(value,
                                 clonedValue);
                return clonedValue;
            }

            return trackedValue;
        }

        private static void CopyPrimitiveKeyAndPrimitiveValue<TKey, TValue>(Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> clone)
        {
            foreach (var kvp in source)
            {
                clone.Add(kvp.Key,
                          kvp.Value);
            }
        }

        private static void CopyObjectKeyAndPrimitiveValue<TKey, TValue>(Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> clone, ObjectClonerReferenceTracker refTracker)
            where TKey : class
        {
            foreach (var kvp in source)
            {
                clone.Add(CloneValueAndTrack(kvp.Key, refTracker),
                          kvp.Value);
            }
        }

        private static void CopyPrivitiveKeyAndObjectValue<TKey, TValue>(Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> clone, ObjectClonerReferenceTracker refTracker)
            where TValue : class
        {
            foreach (var kvp in source)
            {
                if (kvp.Value == null)
                {
                    clone.Add(kvp.Key,
                              default(TValue));
                }
                else
                {
                    clone.Add(kvp.Key,
                              CloneValueAndTrack(kvp.Value, refTracker));
                }
            }
        }

        private static void CopyObjectKeyAndObjectValue<TKey, TValue>(Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> clone, ObjectClonerReferenceTracker refTracker)
            where TKey : class
            where TValue : class
        {
            foreach (var kvp in source)
            {
                var clonedKey = CloneValueAndTrack(kvp.Key, refTracker);

                if (kvp.Value == null)
                {
                    clone.Add(clonedKey,
                              default(TValue));
                }
                else
                {
                    clone.Add(clonedKey,
                              CloneValueAndTrack(kvp.Value, refTracker));
                }
            }
        }
    }
}
