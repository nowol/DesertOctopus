using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesertOctopus.Cloning;

namespace DesertOctopus.Utilities
{
    internal static class IQueryableMIH
    {
        public static MethodInfo IsGenericIQueryableType()
        {
            return typeof(IQueryableCloner).GetMethod("IsGenericIQueryableType", new [] { typeof(Type) });
        }
    }
}
