using BuildingBlocks.SharedEntities;

namespace Promotion_Service.Entities
{
    /// <summary>
    /// Tracks coupon usage by users.
    /// </summary>
    public class CouponUsage : BaseEntity
    {
        public int CouponId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public decimal DiscountApplied { get; set; }
        public string? IpAddress { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Coupon Coupon { get; set; } = null!;
    }
}
