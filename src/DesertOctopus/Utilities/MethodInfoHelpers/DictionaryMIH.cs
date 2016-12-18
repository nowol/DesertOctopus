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
    internal static class DictionaryMih
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

        public static MethodInfo IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties()
        {
            return typeof(DictionaryHelper).GetMethod(nameof(DictionaryHelper.IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties),
                                                      BindingFlags.Static | BindingFlags.NonPublic,
                                                      null,
                                                      CallingConventions.Any,
                                                      new[]
                                                      {
                                                          typeof(object)
                                                      },
                                                      new ParameterModifier[0]);
        }

        public static MethodInfo IsDefaultEqualityComparer()
        {
            return typeof(DictionaryHelper).GetMethod(nameof(DictionaryHelper.IsDefaultEqualityComparer),
                                                      BindingFlags.Static | BindingFlags.NonPublic,
                                                      null,
                                                      CallingConventions.Any,
                                                      new[]
                                                      {
                                                          typeof(Type),
                                                          typeof(object)
                                                      },
                                                      new ParameterModifier[0]);
        }
    }
}
