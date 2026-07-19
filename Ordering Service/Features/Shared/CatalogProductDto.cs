namespace Ordering_Service.Features.Shared
{
    public class CatalogProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool InStock { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }

        public bool HasDiscount => DiscountedPrice.HasValue && DiscountedPrice.Value > 0 && DiscountedPrice.Value < Price;
    }
}
