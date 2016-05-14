using System;
using System.Reflection;
using DesertOctopus.Cloning;

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
