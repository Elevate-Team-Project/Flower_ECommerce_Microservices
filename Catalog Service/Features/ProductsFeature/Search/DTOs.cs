namespace Catalog_Service.Features.ProductsFeature.Search
{
    public class DTOs
    {
        public string? Name { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public List<int>? CategoryIds { get; set; }
        public List<int>? OccasionIds { get; set; }
    }
}
