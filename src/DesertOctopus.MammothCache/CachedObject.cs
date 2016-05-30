using System;
using System.Linq;

namespace DesertOctopus.MammothCache
{
    internal sealed class CachedObject
    {
        public int ObjectSize { get; set; }

        public object Value { get; set; }

        public string Key { get; set; }
    }
}