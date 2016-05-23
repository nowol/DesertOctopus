using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public sealed class Category : BaseEntity
    {
        public Category()
        {
            ChildCategories = new List<Category>();
            DisplayName = new Localizable<string>();
        }

        public string Id { get; set; }

        public ICollection<Category> ChildCategories { get; set; }
        public Localizable<string> DisplayName { get; set; }
        public string DefinitionName { get; set; }
        public Localizable<string> Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public int? SequenceNumber { get; set; }
        public bool HiddenInScope { get; set; }
        public bool Active { get; set; }
        public string CatalogId { get; set; }
        public bool? IncludeInSearch { get; set; }
        public string PrimaryParentCategoryId { get; set; }
        public ICollection<ProductEntityRelationship> Relationships { get; set; }
    }
}
