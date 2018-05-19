using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Tests.Models
{
    [Serializable]
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
