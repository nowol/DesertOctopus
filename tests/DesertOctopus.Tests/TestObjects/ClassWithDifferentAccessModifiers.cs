using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public class ClassWithDifferentAccessModifiers
    {
#pragma warning disable SA1401 // Fields must be private
        public int PublicFieldValue;
        internal int InternalFieldValue;
#pragma warning restore SA1401 // Fields must be private
#pragma warning disable SA1306 // Field names must begin with lower-case letter
        private int PrivateFieldValue;
#pragma warning restore SA1306 // Field names must begin with lower-case letter

        public int PublicPropertyValue { get; set; }
        private int PrivatePropertyValue { get; set; }
        internal int InternalPropertyValue { get; set; }

        public void SetPrivateFieldValue(int value)
        {
            PrivateFieldValue = value;
        }
        public int GetPrivateFieldValue()
        {
            return PrivateFieldValue;
        }

        public void SetPrivatePropertyValue(int value)
        {
            PrivatePropertyValue = value;
        }
        public int GetPrivatePropertyValue()
        {
            return PrivatePropertyValue;
        }
    }
}
