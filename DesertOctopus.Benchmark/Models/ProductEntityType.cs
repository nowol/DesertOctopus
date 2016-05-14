using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public enum ProductEntityType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Category = 1,

        [EnumMember]
        Product = 2,

        [EnumMember]
        Variant = 4,
    }
}