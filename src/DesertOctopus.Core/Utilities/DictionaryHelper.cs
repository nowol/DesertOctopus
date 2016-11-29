using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Polyfills;

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
                if (targetType.GetTypeInfo().IsGenericType
                    && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    return targetType;
                }

                targetType = targetType.GetTypeInfo().BaseType;
            }
            while (targetType != null);

            if (throwIfNotADictionary)
            {
                throw new InvalidOperationException("Type " + type + " is not a Dictionary<,>");
            }

            return null;
        }

        private static readonly ConcurrentDictionary<Type, Type> DictionaryTypeForType = new ConcurrentDictionary<Type, Type>();

        internal static bool IsObjectADictionaryWithDefaultComparer(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var dictType = DictionaryTypeForType.GetOrAdd(obj.GetType(),
                                                          t =>
                                                          {
                                                              var targetType = obj.GetType();

                                                              do
                                                              {
                                                                  if (targetType.GetTypeInfo().IsGenericType
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

                                                                  targetType = targetType.GetTypeInfo().BaseType;
                                                              }
                                                              while (targetType != null);

                                                              return null;
                                                          });

            if (dictType != null)
            {
                var defaultComparer = DefaultEqualityComparerForType.GetOrAdd(dictType.GetTypeInfo().GenericTypeArguments[0],
                                                                              GetDefaultEqualityComparerForType);
                var dictComparer = GetDictionaryComparer(obj, dictType);

                if (defaultComparer != dictComparer)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private static object GetDictionaryComparer(object obj, Type dictType)
        {
            var fct = GetComparerFromType.GetOrAdd(obj.GetType(),
                                                   type =>
                                                   {
                                                       return GetDictionaryComparer(dictType.GetTypeInfo().GenericTypeArguments[0],
                                                                                    dictType.GetTypeInfo().GenericTypeArguments[1]);
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
            var prop = t.GetProperty("Default", isStatic: true, isPublic: true);
            return prop.GetValue(null, null);
        }
    }
}
