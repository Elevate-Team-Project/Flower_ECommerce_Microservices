namespace Catalog_Service.Features.CategoriesFeature.UpdateCategory
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
