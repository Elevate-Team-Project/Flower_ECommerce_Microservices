using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog_Service.Entities
{
    public class ProductOccasion
    {
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        [ForeignKey(nameof(Occasion))]
        public int OccasionId { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Occasion Occasion { get; set; }
    }
}
