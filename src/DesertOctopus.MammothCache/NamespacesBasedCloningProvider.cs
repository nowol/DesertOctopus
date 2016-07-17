using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Implementation of <see cref="IFirstLevelCacheCloningProvider"/> that always clone objects
    /// </summary>
    public class NamespacesBasedCloningProvider : IFirstLevelCacheCloningProvider
    {
        private readonly ConcurrentBag<string> _namespacesToClone;
        private readonly ConcurrentDictionary<Type, bool> _requireCloning = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespacesBasedCloningProvider"/> class.
        /// </summary>
        /// <param name="namespacesToClone">Objects contained in one of the namespaces (or child of the namespace) will be cloned</param>
        public NamespacesBasedCloningProvider(IEnumerable<string> namespacesToClone)
        {
            _namespacesToClone = new ConcurrentBag<string>(namespacesToClone);
        }

        /// <inheritdoc/>
        public T Clone<T>(T objectToClone)
            where T : class
        {
            return ObjectCloner.Clone(objectToClone);
        }

        /// <inheritdoc/>
        public bool RequireCloning(Type type)
        {
            if (type == null
                || type.Namespace == null)
            {
                return false;
            }

            return _requireCloning.GetOrAdd(type, IsCloningRequired);
        }

        private bool IsCloningRequired(Type type)
        {
            return _namespacesToClone.Any(x => type.Namespace.StartsWith(x));
        }
    }
}