using System.Reflection;
using DesertOctopus.Cloning;

namespace DesertOctopus.Utilities.MethodInfoHelpers
{
    /// <summary>
    /// Helper class for ObjectClonerReferenceTracker MethodInfo
    /// </summary>
    internal static class ObjectClonerReferenceTrackerMIH
    {
        /// <summary>
        /// Calls ObjectClonerReferenceTracker.IsSourceObjectTracked
        /// </summary>
        /// <returns>The method info for ObjectClonerReferenceTracker.IsSourceObjectTracked</returns>
        public static MethodInfo IsSourceObjectTracked()
        {
            return typeof(ObjectClonerReferenceTracker).GetMethod(nameof(ObjectClonerReferenceTracker.IsSourceObjectTracked));
        }

        /// <summary>
        /// Calls ObjectClonerReferenceTracker.Track
        /// </summary>
        /// <returns>The method info for ObjectClonerReferenceTracker.Track</returns>
        public static MethodInfo Track()
        {
            return typeof(ObjectClonerReferenceTracker).GetMethod(nameof(ObjectClonerReferenceTracker.Track));
        }

        /// <summary>
        /// Calls ObjectClonerReferenceTracker.GetEquivalentTargetObject
        /// </summary>
        /// <returns>The method info for ObjectClonerReferenceTracker.GetEquivalentTargetObject</returns>
        public static MethodInfo GetEquivalentTargetObject()
        {
            return typeof(ObjectClonerReferenceTracker).GetMethod(nameof(ObjectClonerReferenceTracker.GetEquivalentTargetObject));
        }
    }
}
