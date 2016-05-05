using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class ListMIH
    {
        public static MethodInfo ObjectListAdd()
        {
            return typeof(List<object>).GetMethod("Add", new []{typeof(object)});
        }
    }
}
