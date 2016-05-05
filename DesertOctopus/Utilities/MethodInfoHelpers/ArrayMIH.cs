using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class ArrayMIH
    {
        public static MethodInfo CreateInstance()
        {
            return typeof(Array).GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Type), typeof(int[]) }, null);
        }

        public static MethodInfo SetValueRank()
        {
            return typeof(Array).GetMethod("SetValue", new[] { typeof(object), typeof(int[]) });
        }

        public static MethodInfo GetValueRank()
        {
            return typeof(Array).GetMethod("GetValue", new[] { typeof(int[]) });
        }

        public static MethodInfo GetLength()
        {
            return typeof(Array).GetMethod("GetLength");
        }

        public static MethodInfo SetValue()
        {
            return typeof(Array).GetMethod("SetValue", new[] { typeof(object), typeof(int) });
        }

        public static MethodInfo GetValue()
        {
            return typeof(Array).GetMethod("GetValue", new[] { typeof(int) });
        }

        public static MethodInfo Clone()
        {
            return typeof(Array).GetMethod("Clone");
        }
    }
}
