using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public class Product : BaseEntity
    {
        public Product()
        {
            Variants = new Collection<Variant>();
        }

        private ICollection<ProductPriceEntry> _prices;
        public string CatalogId { get; set; }
        public string DefinitionName { get; set; }
        public Localizable<string> DisplayName { get; set; }
        public decimal? ListPrice { get; set; }
        public string Id { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public Localizable<string> Description { get; set; }
        private Category _primaryParentCategory;
        public ICollection<Category> ParentCategories { get; set; }

        public ICollection<ProductPriceEntry> Prices
        {
            get { return _prices = _prices ?? new List<ProductPriceEntry>(); }
            set { _prices = value; }
        }

        public Category PrimaryParentCategory
        {
            get { return _primaryParentCategory; }
            set
            {
                _primaryParentCategory = value;

                if (value != null)
                {
                    PrimaryParentCategoryId = value.Id;
                }
                else
                {
                    PrimaryParentCategoryId = null;
                }
            }
        }

        public string PrimaryParentCategoryId { get; set; }
        public ICollection<Variant> Variants { get; set; }
        public string Sku { get; set; }
        public bool? Active { get; set; }
        public ICollection<ProductEntityRelationship> Relationships { get; set; }
        public int? SequenceNumber { get; set; }
        public bool HiddenInScope { get; set; }
        public bool? IncludeInSearch { get; set; }
        public DateTime? NewProductDate { get; set; }
        public bool IsOverridden { get; set; }
        public DateTime? LastPublishedDate { get; set; }
        public string TaxCategory { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal? ItemFormat { get; set; }
        public string SellingMethod { get; set; }
        public string Brand { get; set; }
    }
}
