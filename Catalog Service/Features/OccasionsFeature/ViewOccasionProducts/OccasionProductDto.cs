namespace Catalog_Service.Features.OccasionsFeature.GetAllOccasions
{
    public class OccasionProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }
        public List<string> Images { get; set; }
    }

}
