using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public class ProductEntityRelationshipComparer : IEqualityComparer<ProductEntityRelationship>
    {
        public bool Equals(ProductEntityRelationship x, ProductEntityRelationship y)
        {
            return x.Id == y.Id;
        }
        public int GetHashCode(ProductEntityRelationship obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}