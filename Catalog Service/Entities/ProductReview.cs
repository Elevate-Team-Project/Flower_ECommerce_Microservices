using BuildingBlocks.SharedEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog_Service.Entities
{
    public class ProductReview : BaseEntity
    {
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public string UserId { get; set; } // Logic Link only
        public string UserName { get; set; } // Cached name to avoid calling Identity Service
        public int Rating { get; set; }
        public string Comment { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}
