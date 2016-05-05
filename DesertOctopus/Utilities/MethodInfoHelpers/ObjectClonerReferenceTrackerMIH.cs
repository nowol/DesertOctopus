using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.ObjectCloner;

namespace DesertOctopus.Utilities.MethodInfoHelpers
{
    internal static class ObjectClonerReferenceTrackerMIH
    {
        public static MethodInfo IsSourceObjectTracked()
        {
            return typeof(ObjectClonerReferenceTracker).GetMethod("IsSourceObjectTracked");
        }

        public static MethodInfo Track()
        {
            return typeof(ObjectClonerReferenceTracker).GetMethod("Track");
        }

        public static MethodInfo GetEquivalentTargetObject()
        {
            return typeof(ObjectClonerReferenceTracker).GetMethod("GetEquivalentTargetObject");
        }
    }
}
