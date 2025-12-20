namespace Catalog_Service.Features.ProductsFeature.Search
{
    public class ViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public int? BrandId { get; set; }
        public string? BrandName { get; set; }

        public string? MainImageUrl { get; set; }

        public List<string> OccasionNames { get; set; }
    }
}
