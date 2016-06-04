using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Implementation of <see cref="IFirstLevelCacheCloningProvider"/> that do not clone objects
    /// </summary>
    public class NoCloningProvider : IFirstLevelCacheCloningProvider
    {
        /// <inheritdoc/>
        public T Clone<T>(T objectToClone)
            where T : class
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool RequireCloning(Type type)
        {
            return false;
        }
    }
}
