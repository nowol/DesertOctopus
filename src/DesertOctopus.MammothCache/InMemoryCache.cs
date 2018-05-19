using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Providers;

namespace DesertOctopus.MammothCache
{
    internal class InMemoryCache : IDisposable
    {
        private readonly TimeSpan _cleanupInterval;
        private readonly int _maximumMemorySize;
        private readonly ICurrentDateAndTimeProvider _currentDateAndTimeProvider;
        private readonly Dictionary<string, CachedObject> _cache = new Dictionary<string, CachedObject>();
        private readonly LinkedList<string> _orderedCacheKeys = new LinkedList<string>();
        private readonly object _syncRoot = new object();

        private readonly LongCounter _estimatedMemorySize = new LongCounter();
        private readonly System.Threading.Timer _cleanUpTimer;
        private readonly Action<object> _startCleanUpAction;

        private bool _isDisposed = false;
        private bool _cleanUpInProgress = false;
        private DateTime _lastExpirationScan;

        public InMemoryCache(TimeSpan cleanupInterval, int maximumMemorySize)
            : this(cleanupInterval, maximumMemorySize, DateNowProvider.Instance)
        {
        }

        internal InMemoryCache(TimeSpan cleanupInterval, int maximumMemorySize, ICurrentDateAndTimeProvider currentDateAndTimeProvider)
        {
            _cleanupInterval = cleanupInterval;
            _maximumMemorySize = maximumMemorySize;
            _currentDateAndTimeProvider = currentDateAndTimeProvider;

            _cleanUpTimer = new System.Threading.Timer(CleanUpTimerOnElapsed, null, _cleanupInterval, Timeout.InfiniteTimeSpan);

            _startCleanUpAction = StartCleanUp;

            _lastExpirationScan = _currentDateAndTimeProvider.GetCurrentDateTime();
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _cleanUpTimer.Dispose();
            _isDisposed = true;
        }

        private void GuardDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(InMemoryCache));
            }
        }

        private void DecreaseEstimatedMemory(CachedObject cachedObject)
        {
            _estimatedMemorySize.Substract(cachedObject.ObjectSize);
        }

        private void CleanUpTimerOnElapsed(object state)
        {
            bool isDisposed = false;
            lock (_syncRoot)
            {
                isDisposed = _isDisposed;
            }

            if (!isDisposed)
            {
                lock (_syncRoot)
                {
                    if (!_cleanUpInProgress)
                    {
                        _cleanUpTimer.Change(Timeout.Infinite,
                                             Timeout.Infinite);
                        _cleanUpInProgress = true;
                        _lastExpirationScan = _currentDateAndTimeProvider.GetCurrentDateTime();

                        Task.Factory.StartNew(_startCleanUpAction,
                                              this,
                                              CancellationToken.None,
                                              TaskCreationOptions.DenyChildAttach,
                                              TaskScheduler.Default);
                    }
                }
            }
        }

        private void StartCleanUp(object state)
        {
            bool isDisposed = false;
            lock (_syncRoot)
            {
                isDisposed = _isDisposed;
            }

            if (!isDisposed)
            {
                RemoveExpiredItems();
                RemoveItemsUntilMemoryIsNotOverflowing();

                lock (_syncRoot)
                {
                    _cleanUpInProgress = false;
                    _cleanUpTimer.Change(_cleanupInterval,
                                         Timeout.InfiniteTimeSpan);
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _cache.Count;
                }
            }
        }

        private void RemoveExpiredItems()
        {
            bool isDisposed = false;
            lock (_syncRoot)
            {
                isDisposed = _isDisposed;
            }

            if (!isDisposed)
            {
                List<string> keysToScan;
                lock (_syncRoot)
                {
                    keysToScan = _cache.Keys.ToList(); // can we avoid copying the whole list while being threadsafe? can we run out of memory because of the copy?
                }

                var now = _currentDateAndTimeProvider.GetCurrentDateTime();

                foreach (var key in keysToScan)
                {
                    CachedObject cachedObject;
                    lock (_syncRoot)
                    {
                        _cache.TryGetValue(key, out cachedObject);
                    }

                    if (cachedObject?.ExpireAt != null
                        && cachedObject.ExpireAt < now)
                    {
                        Remove(key);
                    }
                }
            }
        }

        private void RemoveItemsUntilMemoryIsNotOverflowing()
        {
            // remove oldest objects from cache
            while (_estimatedMemorySize.Get() > _maximumMemorySize && Count > 0)
            {
                string keyToRemove = null;
                lock (_syncRoot)
                {
                    if (_orderedCacheKeys.Count > 0)
                    {
                        keyToRemove = _orderedCacheKeys.First.Value;
                    }
                }

                if (keyToRemove != null)
                {
                    Remove(keyToRemove);
                }
            }
        }

        public void Remove(string key)
        {
            GuardDisposed();

            lock (_syncRoot)
            {
                RemoveNoLock(key);
            }
        }

        private void RemoveNoLock(string key)
        {
            if (_cache.TryGetValue(key, out CachedObject cachedObject))
            {
                DecreaseEstimatedMemory(cachedObject);

                _orderedCacheKeys.Remove(cachedObject.OrderedNode);
                _cache.Remove(key);
            }
        }

        public void RemoveAll()
        {
            GuardDisposed();

            lock (_syncRoot)
            {
                _cache.Clear();
                _orderedCacheKeys.Clear();
                _estimatedMemorySize.Set(0);
            }
        }

        public void Add(string key,
                        CachedObject objectToCache)
        {
            GuardDisposed();

            lock (_syncRoot)
            {
                if (objectToCache == null)
                {
                    return;
                }

                RemoveNoLock(key);

                objectToCache.OrderedNode = _orderedCacheKeys.AddLast(key);
                _cache.Add(key, objectToCache);
                _estimatedMemorySize.Add(objectToCache.ObjectSize);

                if (_estimatedMemorySize.Get() > _maximumMemorySize && !_cleanUpInProgress)
                {
                    // this will requeue the timer if Add is called back-to-back however it will call the clean up immediately if the time since last clean up is too large
                    var now = _currentDateAndTimeProvider.GetCurrentDateTime();

                    if (_cleanupInterval < now - _lastExpirationScan)
                    {
                        _cleanUpTimer.Change(TimeSpan.FromSeconds(0), Timeout.InfiniteTimeSpan);
                    }
                    else
                    {
                        _cleanUpTimer.Change(TimeSpan.FromSeconds(0.5), Timeout.InfiniteTimeSpan);
                    }
                }
            }
        }

        public CachedObject Get(string key)
        {
            if (_cache.TryGetValue(key, out CachedObject cachedObject))
            {
                if (cachedObject.ExpireAt != null
                    && cachedObject.ExpireAt < _currentDateAndTimeProvider.GetCurrentDateTime())
                {
                    // key is expired
                    Remove(key);
                }
                else
                {
                    return cachedObject;
                }
            }

            return null;
        }

        public long EstimatedMemorySize => _estimatedMemorySize.Get();


        #region internal methods for tests

        internal int OrderedCacheKeysCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _orderedCacheKeys.Count;
                }
            }
        }

        internal string OrderedCacheKeysGet(string key)
        {
            lock (_syncRoot)
            {
                return _orderedCacheKeys.FirstOrDefault(x => x == key);
            }
        }

        internal void SetLastExpirationScan(DateTime value)
        {
            lock (_syncRoot)
            {
                _lastExpirationScan = value;
            }
        }

        #endregion
    }
}