using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public class SomeBaseClass
    {
    }

    [Serializable]
    public class GenericBaseClass<T> : SomeBaseClass
    {
        public T Value { get; set; }
    }

    [Serializable]
    public class ClassWithGenericInt : GenericBaseClass<int>
    {
        public ClassWithGenericInt()
        {

        }

        public ClassWithGenericInt(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ClassWithGenericInt;
            if (other == null)
            {
                return false;
            }

            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return "ClassWithGenericInt".GetHashCode() + Value.GetHashCode();
        }
    }

    [Serializable]
    public class ClassWithGenericDouble: GenericBaseClass<double>
    {
        public ClassWithGenericDouble()
        {

        }

        public ClassWithGenericDouble(double value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ClassWithGenericDouble;
            if (other == null)
            {
                return false;
            }

            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return "ClassWithGenericDouble".GetHashCode() + Value.GetHashCode();
        }
    }
}
