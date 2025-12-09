using BuildingBlocks.SharedEntities;

namespace Catalog_Service.Entities
{
    public class Occasion : BaseEntity
    {
        public string Name { get; set; } // e.g., "Mother's Day"
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        // Navigation Property
        public virtual ICollection<ProductOccasion> ProductOccasions { get; set; }
    }
}
