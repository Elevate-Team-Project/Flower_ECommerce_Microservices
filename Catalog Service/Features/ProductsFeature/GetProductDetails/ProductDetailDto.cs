namespace Catalog_Service.Features.ProductsFeature.GetProductDetails
{
    public class ProductDetailDto
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal FinalPrice => Price + Tax;

        public bool InStock { get; set; }
        public int StockQuantity { get; set; }
        public string? BrandName { get; set; }
        public string CategoryName { get; set; }

        public List<string> Images { get; set; } = new();
    }
}
