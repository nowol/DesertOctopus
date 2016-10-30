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
        //private readonly List<object> _trackedObjects2;
        private int _numberOfTrackedObject = 0;

        public const byte Value0 = 0;

        public const byte Value1 = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerObjectTracker"/> class.
        /// </summary>
        public SerializerObjectTracker()
        {
            _trackedObjects = new Dictionary<object, int>();
            //_trackedObjects2 = new List<object>();
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

            //_trackedObjects2.Add(obj);
            _numberOfTrackedObject++;

            if (obj.GetType().IsClass
                && !_trackedObjects.ContainsKey(obj))
            {
                _trackedObjects.Add(obj, _numberOfTrackedObject - 1);
            }
        }

        /// <summary>
        /// Detect if a type can be tracked
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>True if the type can be tracked otherwise false.</returns>
        public bool CanBeTracked(Type type)
        {
            return type.IsClass;
        }

        /// <summary>
        /// Gets the number of tracked objects
        /// </summary>
        public int NumberOfTrackedObjects
        {
            get { return _trackedObjects.Count; }
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

            return null;
        }

        /// <summary>
        /// Gets or sets the byte buffer
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Ensure that the byte buffer is at least <paramref name="size"/> big
        /// </summary>
        /// <param name="size">Desired size for the buffer</param>
        public void EnsureBufferSize(int size)
        {
            if (Buffer == null || Buffer.Length < size)
            {
                Buffer = new byte[size];
            }
        }
    }
}