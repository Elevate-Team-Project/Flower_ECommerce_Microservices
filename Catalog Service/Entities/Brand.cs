using BuildingBlocks.SharedEntities;

namespace Catalog_Service.Entities
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }
    }
}
