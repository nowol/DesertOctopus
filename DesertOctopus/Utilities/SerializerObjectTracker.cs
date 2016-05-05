using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.Utilities
{
    internal class SerializerObjectTracker
    {
        private List<object> TrackedObjects { get; set; }

        public SerializerObjectTracker()
        {
            TrackedObjects = new List<object>();
        }

        public void TrackObject(object obj)
        {
            TrackedObjects.Add(obj);
        }

        public int? GetTrackedObjectIndex(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            int i = 0;
            foreach (var o in TrackedObjects)
            {
                if (o == obj)
                {
                    return i;
                }
                i++;
            }
            return null;
        }
    }
}