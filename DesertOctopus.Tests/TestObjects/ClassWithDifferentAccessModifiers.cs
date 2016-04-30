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
        public int PublicFieldValue;
        private int PrivateFieldValue;
        internal int InternalFieldValue;

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
