using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for ObjectCloner MethodInfo
    /// </summary>
    internal static class ObjectClonerMIH
    {
        /// <summary>
        /// Calls ObjectCloner.CloneImpl
        /// </summary>
        /// <returns>The method info for ObjectCloner.CloneImpl</returns>
        public static MethodInfo CloneImpl()
        {
            return typeof(ObjectCloner).GetMethod("CloneImpl", BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}
