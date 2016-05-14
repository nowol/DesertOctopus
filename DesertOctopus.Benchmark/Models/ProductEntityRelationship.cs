using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public class ProductEntityRelationship : BaseEntity
    {
        public ProductEntityRelationship() { }
        public Localizable<string> Description { get; set; }
        public int Id { get; set; }
        public int Count { get; set; }
        public string Qualifier { get; set; }
        public int SequenceNumber { get; set; }
        public string Type { get; set; }
        public string CatalogId { get; set; }
        public string EntityId { get; set; }
        public ProductEntityType EntityType { get; set; }
        public string ParentEntityId { get; set; }
        public bool IsInherited { get; set; }
    }
}