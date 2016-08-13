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
        public bool[] ByteArray = new bool[0];

        public CachingTestClass()
        {
            Value = Guid.NewGuid();
        }
    }
}
