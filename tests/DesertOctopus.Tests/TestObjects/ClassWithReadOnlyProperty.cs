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
}
