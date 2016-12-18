using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Tests.TestObjects
{
    public class ClassWithReadOnlyProperty<T>
    {
        private readonly T _value;

        public T Value
        {
            get { return _value; }
        }

        public ClassWithReadOnlyProperty(T value)
        {
            _value = value;
        }
    }

    public class ClassWithCSharp6StyleReadOnlyProperty<T>
    {
        public T Value { get; }

        public ClassWithCSharp6StyleReadOnlyProperty(T value)
        {
            Value = value;
        }
    }
    public class ClassWith2Property<T>
    {
        public T Value1 { get; set; }
        public T Value2 { get; set; }

        public ClassWith2Property(T value1, T value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
    }
}
