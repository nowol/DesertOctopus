using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class ICollectionMIH
    {
        public static PropertyInfo Count<T>()
        {
            return typeof(ICollection<T>).GetProperty("Count");
        }
    }
}
