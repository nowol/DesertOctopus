using System;
using System.Linq;
using System.Reflection;
using DesertOctopus.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Serializer MethodInfo
    /// </summary>
    internal static class SerializerMIH
    {
        /// <summary>
        /// Calls Serializer.SetValue
        /// </summary>
        /// <param name="itemType">Type used for the array</param>
        /// <returns>The method info for Serializer.SetValue</returns>
        public static MethodInfo ConvertEnumerableToArray(Type itemType)
        {
            return typeof(Serializer).GetMethod("ConvertEnumerableToArray", BindingFlags.Static | BindingFlags.NonPublic)
                                     .MakeGenericMethod(itemType);
        }

        /// <summary>
        /// Calls Serializer.PrepareObjectForSerialization
        /// </summary>
        /// <returns>The method info for Serializer.PrepareObjectForSerialization</returns>
        public static MethodInfo PrepareObjectForSerialization()
        {
            return typeof(ObjectCleaner).GetMethod("PrepareObjectForSerialization", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Calls Serializer.PrepareObjectForSerialization
        /// </summary>
        /// <returns>The method info for Serializer.PrepareObjectForSerialization</returns>
        public static MethodInfo ConvertObjectToExpectedType()
        {
            return typeof(ObjectCleaner).GetMethod("ConvertObjectToExpectedType", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Calls Serializer.GetTypeSerializer
        /// </summary>
        /// <returns>The method info for Serializer.GetTypeSerializer</returns>
        public static MethodInfo GetTypeSerializer()
        {
            return typeof(Serializer).GetMethod("GetTypeSerializer", BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
