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
            return typeof(SerializerObjectTracker).GetMethod(nameof(SerializerObjectTracker.TrackObject), new Type[] { typeof(object) });
        }

        /// <summary>
        /// Calls SerializerObjectTracker.GetTrackedObjectIndex
        /// </summary>
        /// <returns>The method info for SerializerObjectTracker.GetTrackedObjectIndex</returns>
        public static MethodInfo GetTrackedObjectIndex()
        {
            return typeof(SerializerObjectTracker).GetMethod(nameof(SerializerObjectTracker.GetTrackedObjectIndex));
        }
    }
}
