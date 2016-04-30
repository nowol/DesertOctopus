using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class ListMIH
    {
        public static MethodInfo ObjectListAdd()
        {
            return typeof(List<object>).GetMethod("Add", new []{typeof(object)});
        }
    }
}
