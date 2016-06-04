using System;
using System.Linq;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Implementation of <see cref="IFirstLevelCacheCloningProvider"/> that always clone objects
    /// </summary>
    public class AlwaysCloningProvider : IFirstLevelCacheCloningProvider
    {
        /// <inheritdoc/>
        public T Clone<T>(T objectToClone)
            where T : class
        {
            return ObjectCloner.Clone(objectToClone);
        }

        /// <inheritdoc/>
        public bool RequireCloning(Type type)
        {
            return true;
        }
    }
}