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

            _trackedObjects.Add(obj);
        }

        /// <summary>
        /// Gets the number of tracked objects
        /// </summary>
        public int NumberOfTrackedObjects
        {
            get { return _trackedObjects.Count; }
        }

        /// <summary>
        /// Gets the object at the specified index
        /// </summary>
        /// <param name="index">Index to read from</param>
        /// <returns>The object at the specified index</returns>
        public object GetTrackedObject(int index)
        {
            return _trackedObjects[index];
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