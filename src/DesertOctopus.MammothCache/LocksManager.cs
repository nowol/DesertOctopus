using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache
{
    internal class LocksManager : IDisposable
    {
        private readonly MammothCache _cache;
        private readonly List<IDisposable> _locks = new List<IDisposable>();

        public LocksManager(MammothCache cache)
        {
            _cache = cache;
        }

        public IDisposable AcquireLocks(IEnumerable<string> keys, TimeSpan lockExpiry, TimeSpan timeout)
        {
            foreach (var key in keys)
            {
                _locks.Add(_cache.AcquireLock(key, lockExpiry, timeout));
            }

            return this;
        }

        public async Task<IDisposable> AcquireLocksAsync(IEnumerable<string> keys, TimeSpan lockExpiry, TimeSpan timeout)
        {
            foreach (var key in keys)
            {
                _locks.Add(await _cache.AcquireLockAsync(key, lockExpiry, timeout).ConfigureAwait(false));
            }

            return this;
        }

        public void Dispose()
        {
            foreach (var disposableLock in _locks)
            {
                disposableLock.Dispose();
            }
        }
    }
}
