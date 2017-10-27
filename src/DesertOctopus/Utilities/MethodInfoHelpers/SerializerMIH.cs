using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DesertOctopus.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Serializer MethodInfo
    /// </summary>
    internal static class SerializerMih
    {
        /// <summary>
        /// Calls Serializer.SetValue
        /// </summary>
        /// <param name="itemType">Type used for the array</param>
        /// <returns>The method info for Serializer.SetValue</returns>
        public static MethodInfo ConvertEnumerableToArray(Type itemType)
        {
            return ReflectionHelpers.GetNonPublicStaticMethod(typeof(Serializer), nameof(Serializer.ConvertEnumerableToArray), typeof(IEnumerable)).MakeGenericMethod(itemType);
        }

        /// <summary>
        /// Calls Serializer.PrepareObjectForSerialization
        /// </summary>
        /// <returns>The method info for Serializer.PrepareObjectForSerialization</returns>
        public static MethodInfo PrepareObjectForSerialization()
        {
            return ReflectionHelpers.GetNonPublicStaticMethod(typeof(ObjectCleaner), nameof(ObjectCleaner.PrepareObjectForSerialization), typeof(object));
        }

        /// <summary>
        /// Calls Serializer.PrepareObjectForSerialization
        /// </summary>
        /// <returns>The method info for Serializer.PrepareObjectForSerialization</returns>
        public static MethodInfo ConvertObjectToExpectedType()
        {
            return ReflectionHelpers.GetNonPublicStaticMethod(typeof(ObjectCleaner), nameof(ObjectCleaner.ConvertObjectToExpectedType), typeof(object), typeof(Type));
        }

        /// <summary>
        /// Calls Serializer.GetTypeSerializer
        /// </summary>
        /// <returns>The method info for Serializer.GetTypeSerializer</returns>
        public static MethodInfo GetTypeToObjectSerializer()
        {
            return ReflectionHelpers.GetNonPublicStaticMethod(typeof(Serializer), nameof(Serializer.GetTypeToObjectSerializer), typeof(Type));
        }
    }
}
