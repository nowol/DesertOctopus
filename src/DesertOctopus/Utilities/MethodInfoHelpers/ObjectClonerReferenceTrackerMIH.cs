using System.Reflection;
using DesertOctopus.Cloning;

namespace DesertOctopus.Utilities.MethodInfoHelpers
{
    /// <summary>
    /// Helper class for ObjectClonerReferenceTracker MethodInfo
    /// </summary>
    internal static class ObjectClonerReferenceTrackerMih
    {
        /// <summary>
        /// Calls ObjectClonerReferenceTracker.Track
        /// </summary>
        /// <returns>The method info for ObjectClonerReferenceTracker.Track</returns>
        public static MethodInfo Track()
        {
            return typeof(ObjectClonerReferenceTracker).GetMethod(nameof(ObjectClonerReferenceTracker.Track));
        }
    }
}
