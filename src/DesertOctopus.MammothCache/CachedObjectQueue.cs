using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// List of queued items ordered by age
    /// </summary>
    public sealed class CachedObjectQueue : IDisposable
    {
        private readonly LinkedList<CachedObject> _cachedObjects = new LinkedList<CachedObject>();
        private readonly Semaphore _lockRoot = new Semaphore(1, 1);

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            _lockRoot.Dispose();
        }

        /// <summary>
        /// Gets the number of items in the cache
        /// </summary>
        public int Count
        {
            get
            {
                _lockRoot.WaitOne();
                try
                {
                    return _cachedObjects.Count;
                }
                finally
                {
                    _lockRoot.Release();
                }
            }
        }

        /// <summary>
        /// Remove the oldest item from the list and return it
        /// </summary>
        /// <returns>Removed item</returns>
        public CachedObject Pop()
        {
            _lockRoot.WaitOne();
            try
            {
                CachedObject cachedObject = null;
                if (_cachedObjects.Count > 0)
                {
                    var firstItem = _cachedObjects.First;
                    if (firstItem != null)
                    {
                        cachedObject = firstItem.Value;
                    }

                    _cachedObjects.RemoveFirst();
                }

                return cachedObject;
            }
            finally
            {
                _lockRoot.Release();
            }
        }

        /// <summary>
        /// Add an item to the list
        /// </summary>
        /// <param name="value">Item to add</param>
        public void Add(CachedObject value)
        {
            _lockRoot.WaitOne();
            try
            {
                _cachedObjects.AddLast(value);
            }
            finally
            {
                _lockRoot.Release();
            }
        }

        /// <summary>
        /// Remove an item from the list
        /// </summary>
        /// <param name="value">Item to remove</param>
        public void Remove(CachedObject value)
        {
            _lockRoot.WaitOne();
            try
            {
                _cachedObjects.Remove(value); // todo: can we remove this to be better than O(N) ?
            }
            finally
            {
                _lockRoot.Release();
            }
        }

        /// <summary>
        /// Get keys to remove
        /// </summary>
        /// <param name="memoryToFreeInBytes">Memory to free in bytes</param>
        /// <returns>A list of keys to remove</returns>
        public List<string> GetKeysToEvictDueToMemory(long memoryToFreeInBytes)
        {
            _lockRoot.WaitOne();
            try
            {
                var keys = new List<string>();
                var node = _cachedObjects.First;

                while (memoryToFreeInBytes > 0
                       && node != null)
                {
                    keys.Add(node.Value.Key);
                    memoryToFreeInBytes -= node.Value.ObjectSize;
                    node = node.Next;
                }

                return keys;
            }
            finally
            {
                _lockRoot.Release();
            }
        }
    }
}