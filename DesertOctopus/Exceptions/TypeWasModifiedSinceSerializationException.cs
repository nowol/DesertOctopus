using System;
using System.Linq;
using System.Reflection;
using DesertOctopus.Utilities;

namespace DesertOctopus.Exceptions
{
    [Serializable]
    public class TypeWasModifiedSinceSerializationException : Exception
    {
        internal TypeWasModifiedSinceSerializationException(string message)
            : base(message)
        {
        }

        internal TypeWasModifiedSinceSerializationException(TypeWithHashCode type)
            : base(type.Type.ToString())
        {
        }

        internal static ConstructorInfo GetConstructor()
        {
            return typeof(TypeWasModifiedSinceSerializationException).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                                                     null,
                                                                                     new[]
                                                                                     {
                                                                                         typeof(TypeWithHashCode)
                                                                                     },
                                                                                     new ParameterModifier[0]);
        }
    }
}
