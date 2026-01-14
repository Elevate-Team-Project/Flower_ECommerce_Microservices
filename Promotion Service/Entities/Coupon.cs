using BuildingBlocks.SharedEntities;

namespace Promotion_Service.Entities
{
    /// <summary>
    /// Represents a coupon code that users can apply for discounts.
    /// </summary>
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }

        // Discount Configuration
        public CouponType Type { get; set; } = CouponType.Percentage;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }  // Cap for percentage discounts

        // Requirements
        public decimal? MinOrderAmount { get; set; }
        public string? ApplicableCustomerGroups { get; set; }  // JSON array of group IDs
        public string? ApplicableCategories { get; set; }  // JSON array of category IDs
        public string? ApplicableProducts { get; set; }  // JSON array of product IDs

        // Usage Limits
        public int? MaxTotalUsage { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public int CurrentUsageCount { get; set; } = 0;

        // Validity Period
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        // Admin Notes
        public string? AdminNotes { get; set; }

        // Navigation
        public virtual ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();

        // Computed Properties
        public bool IsExpired => DateTime.UtcNow > ValidUntil;
        public bool IsNotYetValid => DateTime.UtcNow < ValidFrom;
        public bool HasReachedMaxUsage => MaxTotalUsage.HasValue && CurrentUsageCount >= MaxTotalUsage.Value;
    }

    public enum CouponType
    {
        Percentage,
        FixedAmount,
        FreeShipping
    }
}
