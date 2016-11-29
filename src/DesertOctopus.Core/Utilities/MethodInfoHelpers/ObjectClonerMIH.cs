using System.Reflection;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for ObjectCloner MethodInfo
    /// </summary>
    internal static class ObjectClonerMih
    {
        /// <summary>
        /// Calls ObjectCloner.CloneImpl
        /// </summary>
        /// <returns>The method info for ObjectCloner.CloneImpl</returns>
        public static MethodInfo CloneImpl()
        {
            return typeof(ObjectCloner).GetMethod(nameof(ObjectCloner.CloneImpl), BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}
