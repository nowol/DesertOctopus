using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public struct StructForTesting
    {
        public int Value;
    }

    [Serializable]
    public struct StructForTestingWithString
    {
        public string StringValue;
        public ClassWithGenericInt IntWrapper;
    }
}
