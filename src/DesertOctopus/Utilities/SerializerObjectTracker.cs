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
        private readonly Dictionary<object, int> _trackedObjects;
        private readonly List<object> _trackedObjects2;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerObjectTracker"/> class.
        /// </summary>
        public SerializerObjectTracker()
        {
            _trackedObjects = new Dictionary<object, int>();
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
                if (!_trackedObjects.ContainsKey(obj))
                {
                    _trackedObjects.Add(obj,
                                        _trackedObjects2.Count - 1);
                }
            }
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

            int index;
            if (_trackedObjects.TryGetValue(obj, out index))
            {
                return index;
            }

            //int i = 0;
            //foreach (var o in _trackedObjects2)
            //{


            //    if (o == obj || (obj is string && o.Equals(obj)))
            //    {
            //        //int index;
            //        //if (!_trackedObjects.TryGetValue(obj, out index))
            //        //{
            //        //    throw new Exception();
            //        //}

            //        //if (index != i)
            //        //{
            //        //    throw new Exception();
            //        //}

            //        return i;
            //    }

            //    i++;
            //}

            return null;
        }
    }

    /*
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
     */
}