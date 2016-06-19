using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache.Redis
{
    /// <summary>
    /// Provide a way to lock keys in Redis
    /// </summary>
    public sealed class RedisLock : IDisposable
    {
        private const string LockPrefix = "DistributedLock:";
        private readonly RedisConnection _connection;
        private readonly string _lockValue = Guid.NewGuid().ToString();
        private bool _lockIsAcquired = false;
        private string _key;

        internal RedisLock(RedisConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Acquire a lock synchronously
        /// </summary>
        /// <param name="key">Key to lock</param>
        /// <param name="lockExpiry">Time that the lock will be hold</param>
        /// <param name="timeout">Time wait while acquiring the lock</param>
        /// <returns>A disposable object that represents the lock</returns>
        public async Task<IDisposable> AcquireLockAsync(string key,
                                                        TimeSpan lockExpiry,
                                                        TimeSpan timeout)
        {
            _key = key;
            Stopwatch sw = Stopwatch.StartNew();

            while (sw.Elapsed < timeout && !_lockIsAcquired)
            {
                _lockIsAcquired = await _connection.GetRetryPolicyAsync()
                                                   .ExecuteAsync(() => _connection.GetDatabase().LockTakeAsync(LockPrefix + _key, _lockValue, lockExpiry))
                                                   .ConfigureAwait(false);
                if (!_lockIsAcquired)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }

            if (!_lockIsAcquired)
            {
                throw new UnableToAcquireLockException("Key: " + _key);
            }

            return this;
        }

        /// <summary>
        /// Acquire a lock synchronously
        /// </summary>
        /// <param name="key">Key to lock</param>
        /// <param name="lockExpiry">Time that the lock will be hold</param>
        /// <param name="timeout">Time wait while acquiring the lock</param>
        /// <returns>A disposable object that represents the lock as an awaitable task</returns>
        public IDisposable AcquireLock(string key,
                                       TimeSpan lockExpiry,
                                       TimeSpan timeout)
        {
            _key = key;
            Stopwatch sw = Stopwatch.StartNew();

            while (sw.Elapsed < timeout && !_lockIsAcquired)
            {
                _lockIsAcquired = _connection.GetRetryPolicy()
                                             .Execute(() => _connection.GetDatabase().LockTake(LockPrefix + _key, _lockValue, lockExpiry));
                if (!_lockIsAcquired)
                {
                    Thread.Sleep(100);
                }
            }

            if (!_lockIsAcquired)
            {
                throw new UnableToAcquireLockException("Key: " + _key);
            }

            return this;
        }

        /// <summary>
        /// Release the Redis lock
        /// </summary>
        public void Dispose()
        {
            if (_lockIsAcquired)
            {
                _connection.GetRetryPolicy()
                           .Execute(() => _connection.GetDatabase().LockRelease(LockPrefix + _key, _lockValue));
            }
        }
    }
}