using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Dictionary MethodInfo
    /// </summary>
    internal static class DictionaryMIH
    {
        /// <summary>
        /// Calls Dictionary.Add
        /// </summary>
        /// <typeparam name="TKey">TKey can be any type</typeparam>
        /// <typeparam name="TValue">TValue can be any type</typeparam>
        /// <returns>The method info for Dictionary.Add</returns>
        public static MethodInfo Add<TKey, TValue>()
        {
            return typeof(IDictionary<TKey, TValue>).GetMethod(nameof(IDictionary<TKey, TValue>.Add), new[] { typeof(TKey), typeof(TValue) });
        }

        public static MethodInfo Add(Type dictionaryType, Type keyType, Type valueType)
        {
            return dictionaryType.GetMethod("Add", new[] { keyType, valueType });
        }

        public static MethodInfo GetEnumerator(Type dictionaryType)
        {
            Debug.Assert(dictionaryType.IsGenericType && dictionaryType.GetGenericTypeDefinition() == typeof(Dictionary<,>), "Type " + dictionaryType + " is not a dictionary");
            return dictionaryType.GetMethod("GetEnumerator");
        }

        public static MethodInfo IsObjectADictionaryWithDefaultComparer()
        {
            return typeof(DictionaryHelper).GetMethod(nameof(DictionaryHelper.IsObjectADictionaryWithDefaultComparer),
                                                      BindingFlags.Static | BindingFlags.NonPublic,
                                                      null,
                                                      CallingConventions.Any,
                                                      new[]
                                                      {
                                                          typeof(object)
                                                      },
                                                      new ParameterModifier[0]);
        }
    }
}
