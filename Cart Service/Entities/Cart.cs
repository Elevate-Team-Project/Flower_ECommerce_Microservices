using BuildingBlocks.SharedEntities;

namespace Cart_Service.Entities
{
    public class Cart : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string? CouponCode { get; set; }

        // Navigation Property: One-to-Many relationship with CartItem
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Domain Logic Helper (Optional but recommended)
        public decimal TotalPrice => Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
