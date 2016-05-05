using System.Reflection;

namespace DesertOctopus.Utilities
{
    internal static class ObjectClonerMIH
    {
        public static MethodInfo CloneImpl()
        {
            return typeof(Cloning.ObjectCloner).GetMethod("CloneImpl", BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}
