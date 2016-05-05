using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class SerializerObjectTrackerMIH
    {
        public static MethodInfo TrackObject()
        {
            return typeof(SerializerObjectTracker).GetMethod("TrackObject", new Type[] { typeof(object) });
        }

        public static MethodInfo GetTrackedObjectIndex()
        {
            return typeof(SerializerObjectTracker).GetMethod("GetTrackedObjectIndex");
        }
    }
}
