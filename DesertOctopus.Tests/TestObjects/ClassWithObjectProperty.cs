using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public class ClassWithObjectProperty
    {
        public Object Obj { get; set; }
    }
}
