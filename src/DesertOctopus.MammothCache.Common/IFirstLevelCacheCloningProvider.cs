using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    /// <summary>
    /// Represents the contract to implements for classes that wishes to provide a cloning provider
    /// </summary>
    public interface IFirstLevelCacheCloningProvider
    {
        /// <summary>
        /// Detect if a type needs to be cloned before being returned from the first level cache
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>True if the object needs to be cloned. Otherwise false.</returns>
        bool RequireCloning(Type type);

        /// <summary>
        /// Clone an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="objectToClone">Object to clone</param>
        /// <returns>The cloned object.</returns>
        T Clone<T>(T objectToClone)
            where T : class;
    }
}
