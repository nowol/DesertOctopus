using System;
using System.Linq;
using System.Reflection;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class CopyReadOnlyFieldMethodInfo
    {
        private static readonly MethodInfo Method = typeof(CopyReadOnlyFieldMethodInfo).GetMethod("CopyReadonlyField", BindingFlags.NonPublic | BindingFlags.Static);

        public static MethodInfo GetMethodInfo()
        {
            return Method;
        }

        private static void CopyReadonlyField(FieldInfo field, object value, object target)
        {
            // using reflection to copy readonly fields.  It's slower but it's the only choice
            field.SetValue(target, value);
        }
    }
}