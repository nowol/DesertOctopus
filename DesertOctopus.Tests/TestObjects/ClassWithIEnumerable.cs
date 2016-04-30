using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public class ClassWithIEnumerable<T>
    {
        public IEnumerable<T> Items { get; set; }
    }
}
