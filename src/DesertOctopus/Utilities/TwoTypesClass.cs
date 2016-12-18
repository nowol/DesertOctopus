using System;
using System.Linq;

namespace DesertOctopus.Utilities
{
    internal class TwoTypesClass
    {
        public Type Type { get; set; }

        public Type OtherType { get; set; }

        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + Type.GetHashCode();
                if (OtherType != null)
                {
                    hash = (hash * 23) + OtherType.GetHashCode();
                }

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TwoTypesClass);
        }

        public bool Equals(TwoTypesClass obj)
        {
            return obj != null
                   && Type == obj.Type
                   && OtherType == obj.OtherType;
        }
    }
}