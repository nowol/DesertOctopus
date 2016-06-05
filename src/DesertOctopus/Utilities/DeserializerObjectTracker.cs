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
        //private readonly Dictionary<object, int> _trackedObjects;
        private readonly List<object> _trackedObjects2;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializerObjectTracker"/> class.
        /// </summary>
        public DeserializerObjectTracker()
        {
            //_trackedObjects = new Dictionary<object, int>();
            _trackedObjects2 = new List<object>();
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
                _trackedObjects2.Add(obj);
                //_trackedObjects.Add(obj, _trackedObjects.Count);
            }
        }

        /// <summary>
        /// Get the object at index
        /// </summary>
        /// <param name="index">Index of the object</param>
        /// <returns>The object at index</returns>
        public object GetObjectAtIndex(int index)
        {
            //foreach (var kvp in _trackedObjects)
            //{
            //    if (kvp.Value == index)
            //    {
            //        return kvp.Key;
            //    }
            //}

            //return null;
            return _trackedObjects2[index];
        }
    }
}