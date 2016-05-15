using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class to handle Type resolution
    /// </summary>
    internal static class SerializedTypeResolver
    {
        private static readonly ConcurrentDictionary<string, TypeWithHashCode> TypesFromString = new ConcurrentDictionary<string, TypeWithHashCode>();
        private static readonly Lazy<List<TypeReplacement>> TypeReplacements = new Lazy<List<TypeReplacement>>(CreateTypeReplacements);

        /// <summary>
        /// Returns the <see cref="TypeWithHashCode"/> for the specified name
        /// </summary>
        /// <param name="typeFullName">Name of a type</param>
        /// <returns>The <see cref="TypeWithHashCode"/> for the specified name</returns>
        public static TypeWithHashCode GetTypeFromFullName(string typeFullName)
        {
            return TypesFromString.GetOrAdd(typeFullName,
                                            name => new TypeWithHashCode(name));
        }

        /// <summary>
        /// Returns the <see cref="TypeWithHashCode"/> for the specified name
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>The <see cref="TypeWithHashCode"/> for the specified name</returns>
        public static TypeWithHashCode GetTypeFromFullName(Type type)
        {
            return TypesFromString.GetOrAdd(type.AssemblyQualifiedName,
                                            name => new TypeWithHashCode(type));
        }

        /// <summary>
        /// Returns the short name of the type
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>The <see cref="TypeWithHashCode"/> for the specified name</returns>
        public static string GetShortNameFromType(Type type)
        {
            return GetTypeFromFullName(type).ShortTypeName;
        }

        /// <summary>
        /// Returns the hashcode of a type
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>The <see cref="TypeWithHashCode"/> for the specified name</returns>
        public static int GetHashCodeFromType(Type type)
        {
            return GetTypeFromFullName(type).HashCode;
        }

        private static List<TypeReplacement> CreateTypeReplacements()
        {
            var replacements = new List<TypeReplacement>();

            var strType = typeof(string);

            // Shorten the type name written to the stream.
            // Reading a string from the MemoryStream is expensive and having a smaller name provides a nice performance increase (~4-5 times) while having a smaller payload

            // the order of the replacements is important
            replacements.Add(new TypeReplacement { ShortName = "~mcl~", LongName = strType.AssemblyQualifiedName.Substring(strType.FullName.Length) });
            replacements.Add(new TypeReplacement { ShortName = "~v~", LongName = ", Version=" });
            replacements.Add(new TypeReplacement { ShortName = "~c=n~", LongName = ", Culture=neutral" });
            replacements.Add(new TypeReplacement { ShortName = "~pkt~", LongName = ", PublicKeyToken=" });
            replacements.Add(new TypeReplacement { ShortName = "~SCG~", LongName = "System.Collections.Generic." });
            replacements.Add(new TypeReplacement { ShortName = "~SC~", LongName = "System.Collections." });
            replacements.Add(new TypeReplacement { ShortName = "~S~", LongName = "System." });
            replacements.Add(new TypeReplacement { ShortName = "~M~", LongName = "Microsoft." });

            return replacements;
        }

        /// <summary>
        /// Apply the type replacements
        /// </summary>
        /// <param name="typeName">Name of a type</param>
        /// <returns>The replaced name</returns>
        public static string ApplyTypeReplacements(string typeName)
        {
            foreach (var tr in TypeReplacements.Value)
            {
                typeName = typeName.Replace(tr.LongName, tr.ShortName);
            }

            return typeName;
        }

        /// <summary>
        /// Undo the type replacements
        /// </summary>
        /// <param name="typeName">Name of a type</param>
        /// <returns>The original name</returns>
        public static string RevertTypeReplacements(string typeName)
        {
            for (var i = TypeReplacements.Value.Count - 1; i >= 0; i--)
            {
                var tr = TypeReplacements.Value[i];
                typeName = typeName.Replace(tr.ShortName, tr.LongName);
            }

            return typeName;
        }

        private class TypeReplacement
        {
            public string ShortName { get; set; }

            public string LongName { get; set; }
        }
    }
}
