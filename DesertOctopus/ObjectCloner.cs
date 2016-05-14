using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DesertOctopus.Cloning;
using DesertOctopus.Serialization;
using DesertOctopus.Utilities;

namespace DesertOctopus
{
    public class ObjectCloner
    {
        private static readonly ConcurrentDictionary<Type, Func<object, ObjectClonerReferenceTracker, object>> Cloners = new ConcurrentDictionary<Type, Func<object, ObjectClonerReferenceTracker, object>>();


        public static T Clone<T>(T obj)
            where T: class
        {
            if (obj == null)
            {
                return default(T);
            }

            object objToClone = ObjectCleaner.PrepareObjectForSerialization(obj);

            var refTracker = new ObjectClonerReferenceTracker();
            var clone = CloneImpl(objToClone.GetType())(objToClone, refTracker);

            var queryable = IQueryableCloner.GetInterfaceType(obj.GetType(), typeof(IQueryable<>));

            if (queryable != null)
            {
                var genericArgumentType = queryable.GetGenericArguments()[0];
                return (T)Deserializer.ConvertObjectToIQueryable(clone, typeof(IQueryable<>).MakeGenericType(genericArgumentType));
            }

            return (T)clone;
        }

        internal static void ClearTypeCache()
        {
            Cloners.Clear();
        }

        internal static Func<object, ObjectClonerReferenceTracker, object> CloneImpl(Type type)
        {
            return Cloners.GetOrAdd(type, t => GenerateCloner(type));
        }

        private static Func<object, ObjectClonerReferenceTracker, object> GenerateCloner(Type sourceType)
        {
            ValidateSupportedTypes(sourceType);

            ParameterExpression sourceParameter = Expression.Parameter(typeof(object), "sourceParam");
            ParameterExpression refTrackerParam = Expression.Parameter(typeof(ObjectClonerReferenceTracker), "refTrackerParam");

            var clone = Expression.Parameter(sourceType, "newInstance");
            var source = Expression.Parameter(sourceType, "source");

            var variables = new List<ParameterExpression>();
            var expressions = new List<Expression>();

            variables.Add(source);
            variables.Add(clone);
            expressions.Add(Expression.Assign(source, Expression.Convert(sourceParameter, sourceType)));

            if (sourceType.IsPrimitive || sourceType.IsValueType || (sourceType == typeof(string)))
            {
                expressions.Add(Expression.Assign(clone, source));
            }
            else if (typeof(ISerializable).IsAssignableFrom(sourceType))
            {
                expressions.Add(ISerializableCloner.GenerateISerializableExpression(variables, source, clone, sourceType, refTrackerParam));
            }
            else if (sourceType == typeof(ExpandoObject))
            {
                expressions.Add(ExpandoCloner.GenerateExpandoObjectExpression(variables, source, clone, refTrackerParam));
            }
            else if (sourceType.IsArray)
            {
                expressions.Add(ArrayCloner.GenerateArrayExpression(variables, source, clone, sourceType, refTrackerParam));
            }
            else if (ObjectCleaner.IsEnumeratingType(sourceType))
            {
                expressions.Add(IQueryableCloner.GenerateEnumeratingExpression(variables, source, clone, sourceType, refTrackerParam));
            }
            else
            {
                expressions.Add(ClassCloner.GenerateClassExpressions(variables, sourceType, source, clone, refTrackerParam));
            }

            // Value types require manual boxing
            if (sourceType.IsValueType)
            {
                expressions.Add(Expression.Convert(clone, typeof(object)));
            }
            else
            {
                expressions.Add(clone);
            }

            return Expression.Lambda<Func<object, ObjectClonerReferenceTracker, object>>(Expression.Block(variables, expressions), sourceParameter, refTrackerParam).Compile();
        }

        private static bool IsAGenericList(Type type)
        {
            var isGenericList = false;
            var targetType = type;

            do
            {
                isGenericList = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>);
                targetType = targetType.BaseType;

            } while (!isGenericList && targetType != null);

            return isGenericList;
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

            if (type == typeof(IQueryable))
            {
                throw new NotSupportedException(type.ToString());
            }

            if (type == typeof(IEnumerable))
            {
                throw new NotSupportedException(type.ToString());
            }

            var enumerableType = IQueryableCloner.GetInterfaceType(type, typeof(IEnumerable<>));
            if (enumerableType != null)
            {
                var genericArgument = enumerableType.GetGenericArguments()[0];
                if (genericArgument.IsGenericType
                    && genericArgument.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                {
                    throw new NotSupportedException(type.ToString());
                }
            }

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

        internal static Expression GenerateNullTrackedOrUntrackedExpression(Expression source,
                                                                            Expression clone,
                                                                            Type cloneType,
                                                                            ParameterExpression refTrackerParam,
                                                                            Expression untrackedExpression,
                                                                            Func<Expression, Expression> trackedExpression = null)
        {
            var getEquivalentExpr = Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("GetEquivalentTargetObject"), source);
            if (trackedExpression == null)
            {
                trackedExpression = gett => Expression.Assign(clone, Expression.Convert(gett, cloneType));
            }

            return Expression.IfThenElse(Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("IsSourceObjectTracked"), source),
                                         trackedExpression(getEquivalentExpr),
                                         untrackedExpression);
        }
    }
}
