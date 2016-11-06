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
            return typeof(DeserializerObjectTracker).GetMethod(nameof(DeserializerObjectTracker.GetTrackedObject), BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, new ParameterModifier[0]);
        }

        /// <summary>
        /// Calls Deserializer.TrackedObject
        /// </summary>
        /// <returns>The method info for Deserializer.TrackedObject</returns>
        public static MethodInfo TrackedObject()
        {
            return typeof(DeserializerObjectTracker).GetMethod(nameof(DeserializerObjectTracker.TrackObject), BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(object) }, new ParameterModifier[0]);
        }

        /// <summary>
        /// Calls DeserializerObjectTracker.EnsureBufferSize
        /// </summary>
        /// <returns>The method info for DeserializerObjectTracker.EnsureBufferSize</returns>
        public static MethodInfo EnsureBufferSize()
        {
            return typeof(DeserializerObjectTracker).GetMethod(nameof(DeserializerObjectTracker.EnsureBufferSize));
        }

        /// <summary>
        /// Calls DeserializerObjectTracker.DecimalArray
        /// </summary>
        /// <returns>The method info for DeserializerObjectTracker.DecimalArray</returns>
        public static PropertyInfo DecimalArray()
        {
            return typeof(DeserializerObjectTracker).GetProperty(nameof(DeserializerObjectTracker.DecimalArray));
        }
    }
}
