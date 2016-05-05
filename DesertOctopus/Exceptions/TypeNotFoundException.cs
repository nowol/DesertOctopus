using System;
using System.Linq;

namespace DesertOctopus.Exceptions
{
    [Serializable]
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException(string message)
            : base(message)
        {
        }
    }
}