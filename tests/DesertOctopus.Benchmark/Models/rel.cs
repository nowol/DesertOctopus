using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public class ProductEntityRelationship<T> : ProductEntityRelationship
    {
        public T RelatedEntity { get; set; }
    }
}
