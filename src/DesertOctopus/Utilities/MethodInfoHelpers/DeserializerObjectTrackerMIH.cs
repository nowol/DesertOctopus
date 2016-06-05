﻿using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for SerializerObjectTracker MethodInfo
    /// </summary>
    internal static class DeserializerObjectTrackerMIH
    {
        /// <summary>
        /// Calls SerializerObjectTracker.TrackObject
        /// </summary>
        /// <returns>The method info for SerializerObjectTracker.TrackObject</returns>
        public static MethodInfo TrackObject()
        {
            return typeof(DeserializerObjectTracker).GetMethod("TrackObject", new Type[] { typeof(object) });
        }

        /// <summary>
        /// Calls SerializerObjectTracker.GetTrackedObjectIndex
        /// </summary>
        /// <returns>The method info for SerializerObjectTracker.GetTrackedObjectIndex</returns>
        public static MethodInfo GetTrackedObjectIndex()
        {
            return typeof(DeserializerObjectTracker).GetMethod("GetObjectAtIndex");
        }
    }
}
