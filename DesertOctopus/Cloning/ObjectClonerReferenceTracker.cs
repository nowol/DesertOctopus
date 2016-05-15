using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Class to track object references
    /// </summary>
    internal class ObjectClonerReferenceTracker
    {
        private readonly Dictionary<object, object> _references = new Dictionary<object, object>();

        /// <summary>
        /// Track an object
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">New tracked object</param>
        public void Track(object source, object target)
        {
            if (!IsSourceObjectTracked(source))
            {
                _references.Add(source, target);
            }
        }

        /// <summary>
        /// Detect if an object is already tracked
        /// </summary>
        /// <param name="source">Object used for teting</param>
        /// <returns>True if the source object is already tracked</returns>
        public bool IsSourceObjectTracked(object source)
        {
            if (source == null)
            {
                return false;
            }

            return _references.ContainsKey(source);
        }

        /// <summary>
        /// Returns the equivalent object of the source
        /// </summary>
        /// <param name="source">Source object that we want its equivalent</param>
        /// <returns>The equivalent object of the source</returns>
        public object GetEquivalentTargetObject(object source)
        {
            object target;
            if (_references.TryGetValue(source, out target))
            {
                return target;
            }

            return null;
        }
    }
}