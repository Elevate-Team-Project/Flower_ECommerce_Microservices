using BuildingBlocks.SharedEntities;

namespace Ordering_Service.Entities
{
    public class DiscountUsage : BaseEntity
    {
        public int OrderId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public string DiscountType { get; set; } = string.Empty; // Percentage, FixedAmount
        public decimal DiscountValue { get; set; }

        // Navigation Property
        public virtual Order Order { get; set; } = null!;
    }
}
