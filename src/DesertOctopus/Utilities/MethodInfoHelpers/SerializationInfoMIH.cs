using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for SerializationInfo MethodInfo
    /// </summary>
    internal static class SerializationInfoMih
    {
        /// <summary>
        /// Calls SerializationInfo.Constructor
        /// </summary>
        /// <returns>The method info for SerializationInfo.Constructor</returns>
        public static ConstructorInfo Constructor()
        {
            return typeof(SerializationInfo).GetConstructor(new[] { typeof(Type), typeof(FormatterConverter) });
        }

        /// <summary>
        /// Calls SerializationInfo.AddValue
        /// </summary>
        /// <returns>The method info for SerializationInfo.AddValue</returns>
        public static MethodInfo AddValue()
        {
            return typeof(SerializationInfo).GetMethod(nameof(SerializationInfo.AddValue), new[] { typeof(string), typeof(object) });
        }

        /// <summary>
        /// Calls SerializationInfo.MemberCount
        /// </summary>
        /// <returns>The method info for SerializationInfo.MemberCount</returns>
        public static PropertyInfo MemberCount()
        {
            return typeof(SerializationInfo).GetProperty(nameof(SerializationInfo.MemberCount));
        }

        /// <summary>
        /// Calls SerializationInfo.GetEnumerator
        /// </summary>
        /// <returns>The method info for SerializationInfo.GetEnumerator</returns>
        public static MethodInfo GetEnumerator()
        {
            var getEnumeratorMethodInfo = typeof(SerializationInfo).GetMethod(nameof(SerializationInfo.GetEnumerator), BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public, null, new Type[0], new ParameterModifier[0]);
            Debug.Assert(getEnumeratorMethodInfo != null, "Could not find GetEnumerator method.");

            return getEnumeratorMethodInfo;
        }
    }
}
