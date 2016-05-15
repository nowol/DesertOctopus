using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class to track object references
    /// </summary>
    internal class SerializerObjectTracker
    {
        private readonly List<object> _trackedObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerObjectTracker"/> class.
        /// </summary>
        public SerializerObjectTracker()
        {
            _trackedObjects = new List<object>();
        }

        /// <summary>
        /// Track an object
        /// </summary>
        /// <param name="obj">Object to track</param>
        public void TrackObject(object obj)
        {
            _trackedObjects.Add(obj);
        }

        /// <summary>
        /// Get the index of a tracked object
        /// </summary>
        /// <param name="obj">Object to get the index of</param>
        /// <returns>The index of a tracked object</returns>
        public int? GetTrackedObjectIndex(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            int i = 0;
            foreach (var o in _trackedObjects)
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