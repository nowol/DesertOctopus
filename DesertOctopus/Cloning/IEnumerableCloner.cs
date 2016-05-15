using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Helper class for IEnumerable
    /// </summary>
    internal static class IEnumerableCloner
    {
        /// <summary>
        /// Detect is the type is an IEnumerabler&lt;&gt;
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>True if it is an IEnumerable&lt;&gt; otherwise false</returns>
        internal static bool IsGenericIEnumerableType(Type type)
        {
            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
