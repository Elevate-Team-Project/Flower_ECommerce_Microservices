using BuildingBlocks.FullEntities.Catalog_Service_Entities;
using BuildingBlocks.FullEntities.Catalog_Service_Entities.Occasions;
using BuildingBlocks.SharedEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog_Service.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }

        // Foreign Keys
        [ForeignKey(nameof(Brand))]
        public int? BrandId { get; set; }
        public int CategoryId { get; set; }

        // Navigation
        public virtual Brand Brand { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }
        public virtual ICollection<ProductSpecification> Specifications { get; set; }
        public virtual ICollection<ProductOccasion> ProductOccasions { get; set; }
        public virtual ICollection<ProductReview> Reviews { get; set; }
        public virtual ICollection<PriceHistory> PriceHistories { get; set; }
    }
}
