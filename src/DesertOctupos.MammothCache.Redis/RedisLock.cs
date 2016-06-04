using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DesertOctupos.MammothCache.Redis
{
    public sealed class RedisLock : IDisposable
    {
        private readonly RedisConnection _connection;
        private readonly string _lockValue = Guid.NewGuid().ToString();
        private const string LockPrefix = "LOCK:";
        private bool _lockIsAcquired = false;
        private string _key;

        internal RedisLock(RedisConnection connection)
        {
            _connection = connection;
        }

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