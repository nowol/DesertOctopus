using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
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
