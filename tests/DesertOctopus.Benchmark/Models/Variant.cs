using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public class Variant : BaseEntity
    {
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
        public string ProductId { get; set; }
        public int? SequenceNumber { get; set; }
        public bool? Active { get; set; }
        public bool HiddenInScope { get; set; }
        public ICollection<ProductPriceEntry> Prices
        {
            get { return _prices = _prices ?? new List<ProductPriceEntry>(); }
            set { _prices = value; }
        }
        public string Sku { get; set; }
    }
}