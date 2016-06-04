using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache.Tests.Models
{
    [NotSerializable]
    public class NotSerializableTestClass
    {
        public Guid Value { get; set; }

        public NotSerializableTestClass()
        {
            Value = Guid.NewGuid();
        }
    }
}
