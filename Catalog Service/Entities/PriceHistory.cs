using BuildingBlocks.SharedEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog_Service.Entities
{
    public class PriceHistory : BaseEntity
    {
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string ChangeReason { get; set; } // "Sale", "Seasonal"

        //navigation properties
        public virtual Product Product { get; set; }
    }
}
