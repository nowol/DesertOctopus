using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    public class Wrapper<T>
    {
        public T Value { get; set; }
    }

    public static class Wrapper
    {
        public static Wrapper<T> Create<T>(T value)
        {
            return new Wrapper<T> { Value = value };
        }
    }
}
