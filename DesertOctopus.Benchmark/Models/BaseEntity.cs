using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [DataContract(IsReference = true)]
    [Serializable]
    public abstract class BaseEntity
    {
        private Dictionary<string, object> _bag;

        public BaseEntity()
        {
            TypeName = GetType().Name;
            FullTypeName = GetType().AssemblyQualifiedName;
        }

        public BaseEntity(IDictionary<string, object> sourceProperties)
            : this(null, sourceProperties)
        {
        }

        public BaseEntity(Type type, IDictionary<string, object> sourceProperties)
        {
            if (type == null)
            {
                type = GetType();
            }

            TypeName = type.Name;
            FullTypeName = type.AssemblyQualifiedName;

            if (sourceProperties != null)
            {
                PropertyBag = new Dictionary<string, object>(sourceProperties,
                                                             StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                PropertyBag = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public string FullTypeName { get; set; }
        public string TypeName { get; set; }
        public Dictionary<string, object> PropertyBag
        {
            get { return _bag = _bag ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase); }
            set { _bag = value; }
        }
    }
}