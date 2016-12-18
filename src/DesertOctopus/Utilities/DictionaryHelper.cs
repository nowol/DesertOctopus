using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace DesertOctopus.Utilities
{
    internal static class DictionaryHelper
    {
        private static readonly ConcurrentDictionary<Type, object> DefaultEqualityComparerForType = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, Func<object, object>> GetComparerFromType = new ConcurrentDictionary<Type, Func<object, object>>();

        internal static Type GetDictionaryType(Type type, bool throwIfNotADictionary = true)
        {
            var targetType = type;

            do
            {
                if (targetType.IsGenericType
                    && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    return targetType;
                }

                targetType = targetType.BaseType;
            }
            while (targetType != null);

            if (throwIfNotADictionary)
            {
                throw new InvalidOperationException("Type " + type + " is not a Dictionary<,>");
            }

            return null;
        }

        internal static ConstructorInfo GetComparerConstructor(Type sourceType)
        {
            var ctor = sourceType.GetConstructors()
                                 .SingleOrDefault(x =>
                                 {
                                     var parameters = x.GetParameters();

                                     return parameters.Length == 1
                                            && parameters[0].ParameterType.IsGenericType
                                            && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEqualityComparer<>);
                                 });
            return ctor;
        }

        private static readonly ConcurrentDictionary<Type, Type> DefaultComparerDictionaryTypeForType = new ConcurrentDictionary<Type, Type>();

        internal static bool IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var dictType = DefaultComparerDictionaryTypeForType.GetOrAdd(obj.GetType(),
                                                                         t =>
                                                                         {
                                                                             var targetType = obj.GetType();

                                                                             do
                                                                             {
                                                                                 if (targetType.IsGenericType
                                                                                     && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                                                                 {
                                                                                     var dictionaryFields = InternalSerializationStuff.GetFields(targetType);
                                                                                     var objFields = InternalSerializationStuff.GetFields(obj.GetType());

                                                                                     if (objFields.Count() != dictionaryFields.Count())
                                                                                     {
                                                                                         return null;
                                                                                     }

                                                                                     var ctor = targetType.GetConstructor(new Type[0]);
                                                                                     if (ctor == null)
                                                                                     {
                                                                                         return null;
                                                                                     }
                                                                                     return targetType;
                                                                                 }

                                                                                 targetType = targetType.BaseType;
                                                                             }
                                                                             while (targetType != null);

                                                                             return null;
                                                                         });

            if (dictType != null)
            {
                var dictComparer = GetDictionaryComparer(obj, dictType);
                return IsDefaultEqualityComparer(dictType.GetGenericArguments()[0],
                                                 dictComparer);
            }

            return false;
        }

        private static object GetDefaultEqualityComparer(Type keyType)
        {
            return DefaultEqualityComparerForType.GetOrAdd(keyType,
                                                           GetDefaultEqualityComparerForType);
        }


        internal static bool IsDefaultEqualityComparer(Type keyType, object comparer)
        {
            var defaultComparer = GetDefaultEqualityComparer(keyType);

            if (defaultComparer != comparer)
            {
                return false;
            }

            return true;
        }

        private static object GetDictionaryComparer(object obj, Type dictType)
        {
            var fct = GetComparerFromType.GetOrAdd(obj.GetType(),
                                                   type =>
                                                   {
                                                       return GetDictionaryComparer(dictType.GetGenericArguments()[0],
                                                                                    dictType.GetGenericArguments()[1]);
                                                   });
            return fct(obj);
        }


        private static Func<object, object> GetDictionaryComparer(Type key, Type value)
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(key, value);
            var inputObject = Expression.Parameter(typeof(object), "inputObject");
            var block = Expression.Property(Expression.Convert(inputObject, dictType), "Comparer");

            return Expression.Lambda<Func<object, object>>(block, inputObject).Compile();
        }

        private static object GetDefaultEqualityComparerForType(Type type)
        {
            var t = typeof(EqualityComparer<>).MakeGenericType(type);
            var prop = t.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
            return prop.GetValue(null, null);
        }
    }
}
