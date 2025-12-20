using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog_Service.Features.CategoriesFeature.CreateCategory
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
