using System;
using System.Reflection;
using DesertOctopus.Cloning;

namespace DesertOctopus.Utilities.MethodInfoHelpers
{
    /// <summary>
    /// Helper class for Func MethodInfo
    /// </summary>
    internal static class FuncMih
    {
        /// <summary>
        /// Calls Func.Invoke
        /// </summary>
        /// <returns>The method info for Func.Invoke</returns>
        public static MethodInfo CloneMethodInvoke()
        {
            return ReflectionHelpers.GetPublicMethod(typeof(Func<object, ObjectClonerReferenceTracker, object>), nameof(Func<object, ObjectClonerReferenceTracker, object>.Invoke), typeof(object), typeof(ObjectClonerReferenceTracker));
        }
    }
}
