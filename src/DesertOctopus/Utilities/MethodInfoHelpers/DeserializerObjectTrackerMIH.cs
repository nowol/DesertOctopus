using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for DeserializerObjectTracker MethodInfo
    /// </summary>
    internal static class DeserializerObjectTrackerMih
    {

        /// <summary>
        /// Calls Deserializer.GetTrackedObject
        /// </summary>
        /// <returns>The method info for Deserializer.GetTrackedObject</returns>
        public static MethodInfo GetTrackedObject()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(DeserializerObjectTracker), nameof(DeserializerObjectTracker.GetTrackedObject), typeof(int));
            //return typeof(DeserializerObjectTracker).GetTypeInfo().GetMethod(nameof(DeserializerObjectTracker.GetTrackedObject), BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, new ParameterModifier[0]);
        }

        /// <summary>
        /// Calls Deserializer.TrackedObject
        /// </summary>
        /// <returns>The method info for Deserializer.TrackedObject</returns>
        public static MethodInfo TrackedObject()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(DeserializerObjectTracker), nameof(DeserializerObjectTracker.TrackObject), typeof(object));
        }

        /// <summary>
        /// Calls DeserializerObjectTracker.EnsureBufferSize
        /// </summary>
        /// <returns>The method info for DeserializerObjectTracker.EnsureBufferSize</returns>
        public static MethodInfo EnsureBufferSize()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(DeserializerObjectTracker), nameof(DeserializerObjectTracker.EnsureBufferSize), typeof(int));
        }

        /// <summary>
        /// Calls DeserializerObjectTracker.DecimalArray
        /// </summary>
        /// <returns>The method info for DeserializerObjectTracker.DecimalArray</returns>
        public static PropertyInfo DecimalArray()
        {
            return typeof(DeserializerObjectTracker).GetTypeInfo().GetProperty(nameof(DeserializerObjectTracker.DecimalArray));
        }
    }
}
