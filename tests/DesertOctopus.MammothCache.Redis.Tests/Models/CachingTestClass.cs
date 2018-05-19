using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Redis.Tests.Models
{
    public class CachingTestClass
    {
        public Guid Value { get; set; }
#pragma warning disable SA1401 // Fields must be private
        public bool[] ByteArray = new bool[0];
#pragma warning restore SA1401 // Fields must be private

        public CachingTestClass()
        {
            Value = Guid.NewGuid();
        }
    }
}
