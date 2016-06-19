using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    internal class ValuesToStore<T>
    {
        public Dictionary<CacheItemDefinition, byte[]> InFirstLevelCache { get; set; }

        public Dictionary<CacheItemDefinition, T> InNonSerializableCache { get; set; }

        public Dictionary<CacheItemDefinition, byte[]> InSecondLevelCache { get; set; }

        public ValuesToStore()
        {
            InFirstLevelCache = new Dictionary<CacheItemDefinition, byte[]>();
            InNonSerializableCache = new Dictionary<CacheItemDefinition, T>();
            InSecondLevelCache = new Dictionary<CacheItemDefinition, byte[]>();
        }
    }
}
