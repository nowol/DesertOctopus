using System;
using System.Reflection;
using DesertOctopus.ObjectCloner;

namespace DesertOctopus.Utilities.MethodInfoHelpers
{
    internal static class FuncMIH
    {
        public static MethodInfo CloneMethodInvoke()
        {
            return typeof(Func<object, ObjectClonerReferenceTracker, object>).GetMethod("Invoke");
        }
    }
}
