using BuildingBlocks.SharedEntities;

namespace Catalog_Service.Entities
{
    public class ProductSpecification : BaseEntity
    {
        public int ProductId { get; set; }
        public string SpecKey { get; set; }
        public string SpecValue { get; set; }

        public virtual Product Product { get; set; }
    }
}
