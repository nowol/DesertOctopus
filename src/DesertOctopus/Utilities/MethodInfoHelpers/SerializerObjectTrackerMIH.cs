using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for SerializerObjectTracker MethodInfo
    /// </summary>
    internal static class SerializerObjectTrackerMih
    {
        /// <summary>
        /// Calls SerializerObjectTracker.TrackObject
        /// </summary>
        /// <returns>The method info for SerializerObjectTracker.TrackObject</returns>
        public static MethodInfo TrackObject()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(SerializerObjectTracker), nameof(SerializerObjectTracker.TrackObject), typeof(object));
        }

        /// <summary>
        /// Calls SerializerObjectTracker.GetTrackedObjectIndex
        /// </summary>
        /// <returns>The method info for SerializerObjectTracker.GetTrackedObjectIndex</returns>
        public static MethodInfo GetTrackedObjectIndex()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(SerializerObjectTracker), nameof(SerializerObjectTracker.GetTrackedObjectIndex), typeof(object));
        }

        /// <summary>
        /// Calls SerializerObjectTracker.EnsureBufferSize
        /// </summary>
        /// <returns>The method info for SerializerObjectTracker.EnsureBufferSize</returns>
        public static MethodInfo EnsureBufferSize()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(SerializerObjectTracker), nameof(SerializerObjectTracker.EnsureBufferSize), typeof(int));
        }

        /// <summary>
        /// Calls SerializerObjectTracker.CanBeTracked
        /// </summary>
        /// <returns>The method info for SerializerObjectTracker.CanBeTracked</returns>
        public static MethodInfo CanBeTracked()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(SerializerObjectTracker), nameof(SerializerObjectTracker.CanBeTracked), typeof(Type));
        }
    }
}
