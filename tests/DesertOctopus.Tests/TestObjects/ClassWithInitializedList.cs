using System;
using System.Collections.Generic;
using System.Linq;

namespace SerializerTests.TestObjects
{
    public unsafe class ClassWithInitializedList
    {
        public ClassWithInitializedList()
        {
            Values = new List<int>();
        }
        public List<int> Values { get; set; }
    }
}