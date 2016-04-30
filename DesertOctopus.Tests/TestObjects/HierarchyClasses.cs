using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    public interface IHierarchy
    {
    }

    [Serializable]
    public class BaseHierarchy<T> : IHierarchy
    {
        public T Value { get; set; }
    }

    [Serializable]
    public class ChildIntHierarchy : BaseHierarchy<int>
    {
        public ChildIntHierarchy(int value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class ChildStringHierarchy : BaseHierarchy<string>
    {
        public ChildStringHierarchy(string value)
        {
            Value = value;
        }
    }
}
