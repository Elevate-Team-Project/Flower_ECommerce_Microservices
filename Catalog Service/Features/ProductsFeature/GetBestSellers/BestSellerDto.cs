namespace Catalog_Service.Features.ProductsFeature.GetBestSellers
{
    public record BestSellerDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public decimal? DiscountedPrice { get; init; }
        public decimal? DiscountPercentage { get; init; }
        public string? MainImage { get; init; }
        public List<string> Images { get; init; } = new();
        public string CategoryName { get; init; } = string.Empty;
        public string? BrandName { get; init; }
        public bool InStock { get; init; }
        public int StockQuantity { get; init; }
        public double AverageRating { get; init; }
        public int ReviewCount { get; init; }
    }
}
