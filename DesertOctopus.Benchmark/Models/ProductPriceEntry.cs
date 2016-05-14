using System;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public partial class ProductPriceEntry : BaseEntity
    {
        public string PriceListId { get; set; }
        public decimal Price { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsInherited { get; set; }
        public string PriceListType { get; set; }
        public string PriceListCategory { get; set; }
    }
}