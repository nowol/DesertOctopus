using System;
using System.Linq;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Empty implementation of <see cref="IFirstLevelCache"/> that do not cache anything
    /// </summary>
    public sealed class NullFirstLevelCache : IFirstLevelCache, IDisposable
    {
        /// <summary>
        /// Gets the number of objects stored in the cache
        /// </summary>
        public int NumberOfObjects
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the estimated memory consumed by this instance of <see cref="SquirrelCache"/>
        /// </summary>
        public int EstimatedMemorySize
        {
            get { return 0; }
        }

        /// <inheritdoc/>
        public ConditionalResult<T> Get<T>(string key)
             where T : class
        {
            return ConditionalResult.CreateFailure<T>();
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
        }

        /// <inheritdoc/>
        public void Set(string key, byte[] serializedValue)
        {
        }

        /// <inheritdoc/>
        public void Set(string key, byte[] serializedValue, TimeSpan? ttl)
        {
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
        }
    }
}
