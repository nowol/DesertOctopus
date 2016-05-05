using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.ObjectCloner
{
    internal class ObjectClonerReferenceTracker
    {
        private readonly Dictionary<object, object> _references = new Dictionary<object, object>();

        public void Track(object source, object target)
        {
            if (!IsSourceObjectTracked(source))
            {
                _references.Add(source, target);
            }
        }

        public bool IsSourceObjectTracked(object source)
        {
            if (source == null)
            {
                return false;
            }
            return _references.ContainsKey(source);
        }

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