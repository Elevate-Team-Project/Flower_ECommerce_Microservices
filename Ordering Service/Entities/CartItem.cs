using BuildingBlocks.SharedEntities;

namespace Ordering_Service.Entities
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        // Use decimal for financial calculations to ensure precision
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; } = 1;
        public string? PictureUrl { get; set; }

        // Navigation Property: Link back to the parent Cart
        public virtual Cart Cart { get; set; }
    }
}
