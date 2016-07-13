using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesertOctopus.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Deserializer MethodInfo
    /// </summary>
    internal static class DeserializerMih
    {
        /// <summary>
        /// Calls Deserializer.ConvertObjectToIQueryable
        /// </summary>
        /// <returns>The method info for Deserializer.ConvertObjectToIQueryable</returns>
        public static MethodInfo ConvertObjectToIQueryable()
        {
            return typeof(Deserializer).GetMethod(nameof(Deserializer.ConvertObjectToIQueryable), BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Calls Deserializer.GetTypeDeserializer
        /// </summary>
        /// <returns>The method info for Deserializer.GetTypeDeserializer</returns>
        public static MethodInfo GetTypeDeserializer()
        {
            return typeof(Deserializer).GetMethod(nameof(Deserializer.GetTypeDeserializer), BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Calls Deserializer.GetTrackedObject
        /// </summary>
        /// <returns>The method info for Deserializer.GetTrackedObject</returns>
        public static MethodInfo GetTrackedObject()
        {
            return typeof(Deserializer).GetMethod(nameof(Deserializer.GetTrackedObject), BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(List<object>), typeof(int) }, new ParameterModifier[0]);
        }
    }
}
