using BuildingBlocks.SharedEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog_Service.Entities
{
    public class StockAlert : BaseEntity
    {
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }

        //navigation properties
        public virtual Product Product { get; set; }

    }
}
