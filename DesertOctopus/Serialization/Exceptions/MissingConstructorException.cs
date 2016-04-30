using System;
using System.Linq;

namespace DesertOctopus.Serialization.Exceptions
{
    [Serializable]
    public class MissingConstructorException : Exception
    {
        public MissingConstructorException(string message)
            : base(message)
        {
        }
    }
}