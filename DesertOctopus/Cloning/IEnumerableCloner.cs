using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Cloning
{
    internal static class IEnumerableCloner
    {
        internal static bool IsGenericIEnumerableType(Type sourceType)
        {
            return sourceType.IsGenericType
                   && sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
