using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DesertOctopus.MammothCache
{
    internal sealed class CachedObject
    {
        public int ObjectSize { get; set; }
        public object Value { get; set; }
        public string Key { get; set; }
    }

    internal sealed class CachedObjectQueue
    {
        private readonly LinkedList<CachedObject> _cachedObjects = new LinkedList<CachedObject>();
        private readonly Semaphore _lockRoot = new Semaphore(1, 1);

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
    }
}