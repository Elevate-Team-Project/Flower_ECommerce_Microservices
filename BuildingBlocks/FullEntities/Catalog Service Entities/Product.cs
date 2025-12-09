using BuildingBlocks.FullEntities.Catalog_Service_Entities.Occasions;
using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Catalog_Service_Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? BrandId { get; set; } // Nullable
        public int CategoryId { get; set; }
        public int StockQuantity { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;

        // Navigation Props
        public virtual Brand Brand { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }
        public virtual ICollection<ProductSpecification> Specifications { get; set; }
        public virtual ICollection<ProductReview> Reviews { get; set; }
        public virtual ICollection<ProductOccasion> ProductOccasions { get; set; }
    }
}