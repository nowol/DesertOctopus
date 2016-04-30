using System;
using System.Linq;

namespace SerializerTests.TestObjects
{
    public class ClassWithNonSerializableField
    {
        [NonSerialized]
        private int _nonSerializableProperty;

        public int NonSerializableProperty
        {
            get { return _nonSerializableProperty; }
            set { _nonSerializableProperty = value; }
        }

        public int SerializableProperty { get; set; }
    }
}