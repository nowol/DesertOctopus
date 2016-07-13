using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class to track object references
    /// </summary>
    internal class DeserializerObjectTracker
    {
        private readonly List<object> _trackedObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializerObjectTracker"/> class.
        /// </summary>
        public DeserializerObjectTracker()
        {
            _trackedObjects = new List<object>();
        }

        /// <summary>
        /// Track an object
        /// </summary>
        /// <param name="obj">Object to track</param>
        public void TrackObject(object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj.GetType().IsClass)
            {
                _trackedObjects.Add(obj);
            }
        }

        /// <summary>
        /// Get the object at index
        /// </summary>
        /// <param name="index">Index of the object</param>
        /// <returns>The object at index</returns>
        public object GetObjectAtIndex(int index)
        {
            return _trackedObjects[index];
        }
    }
}