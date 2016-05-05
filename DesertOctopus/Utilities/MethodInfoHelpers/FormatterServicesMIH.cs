using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesertOctopus.Utilities
{
    internal static class FormatterServicesMIH
    {
        public static MethodInfo GetUninitializedObject()
        {
            return typeof(FormatterServices).GetMethod("GetUninitializedObject", BindingFlags.Public | BindingFlags.Static);
        }
    }
}
