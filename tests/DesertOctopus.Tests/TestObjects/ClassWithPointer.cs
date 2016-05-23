using System;
using System.Linq;

namespace SerializerTests.TestObjects
{
    public unsafe class ClassWithPointer
    {
        public int* Value { get; set; }
    }
}