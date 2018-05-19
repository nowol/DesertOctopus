using System;
using System.IO;
using System.Linq;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// This class hold the hash code of a type
    /// </summary>
    internal class TypeWithHashCode
    {
        /// <summary>
        /// Gets the type
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the hash code of the type
        /// </summary>
        public int HashCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWithHashCode"/> class.
        /// </summary>
        /// <param name="typeName">Name of the type</param>
        public TypeWithHashCode(string typeName)
        {
            try
            {
                Type = System.Type.GetType(SerializedTypeResolver.RevertTypeReplacements(typeName), false);
            }
            catch (FileLoadException)
            {
                // specified type was not found
                // we ignore this error because this case is handled elsewhere
                Type = null;
            }

            if (Type != null)
            {
                ComputeSerializationHashCode();
                ComputeShortTypeName();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWithHashCode"/> class.
        /// </summary>
        /// <param name="type">Type to analyze</param>
        public TypeWithHashCode(Type type)
        {
            Type = type;
            ComputeSerializationHashCode();
            ComputeShortTypeName();
        }

        private void ComputeSerializationHashCode()
        {
            if (Type != null)
            {
                var fields = InternalSerializationStuff.GetFields(Type)
                                    .OrderBy(f => f.Name,
                                             StringComparer.Ordinal)
                                    .Select(x => x.Name + x.FieldType.AssemblyQualifiedName);
                string stringToHash = String.Format("{0}{1}",
                                                    Type.AssemblyQualifiedName,
                                                    String.Join(",",
                                                                fields));
                unchecked
                {
                    HashCode = 23;
                    foreach (char c in stringToHash)
                    {
                        HashCode = (HashCode * 31) + c;
                    }
                }
            }
        }

        private string _shortTypeName;

        /// <summary>
        /// Gets the short name of the type
        /// </summary>
        public string ShortTypeName
        {
            get { return _shortTypeName; }
        }

        private void ComputeShortTypeName()
        {
            _shortTypeName = Type.AssemblyQualifiedName;
            _shortTypeName = SerializedTypeResolver.ApplyTypeReplacements(_shortTypeName);
        }
    }
}
