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
            return ReflectionHelpers.GetPublicMethod(typeof(IDictionary<TKey, TValue>), nameof(IDictionary<TKey, TValue>.Add), typeof(TKey), typeof(TValue));
        }

        public static MethodInfo Add(Type dictionaryType, Type keyType, Type valueType)
        {
            return ReflectionHelpers.GetPublicMethod(dictionaryType, "Add", keyType, valueType);
        }

        public static MethodInfo IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties()
        {
            return ReflectionHelpers.GetNonPublicStaticMethod(typeof(DictionaryHelper), nameof(DictionaryHelper.IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties), typeof(object));
        }

        public static MethodInfo IsDefaultEqualityComparer()
        {
            return ReflectionHelpers.GetNonPublicStaticMethod(typeof(DictionaryHelper), nameof(DictionaryHelper.IsDefaultEqualityComparer), typeof(Type), typeof(object));
        }
    }
}
