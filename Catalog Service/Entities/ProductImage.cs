using BuildingBlocks.SharedEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog_Service.Entities
{
    public class ProductImage : BaseEntity
    {
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }

        // Navigation Property
        public virtual Product Product { get; set; }
    }
}
