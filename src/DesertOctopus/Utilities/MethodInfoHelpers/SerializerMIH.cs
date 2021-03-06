﻿using System;
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
            return typeof(Serializer).GetMethod(nameof(Serializer.ConvertEnumerableToArray), BindingFlags.Static | BindingFlags.NonPublic)
                                     .MakeGenericMethod(itemType);
        }

        /// <summary>
        /// Calls Serializer.PrepareObjectForSerialization
        /// </summary>
        /// <returns>The method info for Serializer.PrepareObjectForSerialization</returns>
        public static MethodInfo PrepareObjectForSerialization()
        {
            return typeof(ObjectCleaner).GetMethod(nameof(ObjectCleaner.PrepareObjectForSerialization), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Calls Serializer.PrepareObjectForSerialization
        /// </summary>
        /// <returns>The method info for Serializer.PrepareObjectForSerialization</returns>
        public static MethodInfo ConvertObjectToExpectedType()
        {
            return typeof(ObjectCleaner).GetMethod(nameof(ObjectCleaner.ConvertObjectToExpectedType), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Calls Serializer.GetTypeSerializer
        /// </summary>
        /// <returns>The method info for Serializer.GetTypeSerializer</returns>
        public static MethodInfo GetTypeToObjectSerializer()
        {
            return typeof(Serializer).GetMethod(nameof(Serializer.GetTypeToObjectSerializer), BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
