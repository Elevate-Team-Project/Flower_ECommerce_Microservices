using BuildingBlocks.SharedEntities;

namespace Catalog_Service.Entities
{
    /// <summary>
    /// Represents a promotional offer that applies discounts to products, categories, or occasions.
    /// US-G01: Create Offer
    /// </summary>
    public class Offer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }  // Arabic name
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }

        // Discount Configuration
        public OfferType Type { get; set; } = OfferType.Percentage;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }  // Cap for percentage discounts

        // Targeting
        public OfferTargetType TargetType { get; set; } = OfferTargetType.Product;
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public int? OccasionId { get; set; }

        // Validity Period
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Status Management
        public OfferStatus Status { get; set; } = OfferStatus.Scheduled;
        public int Priority { get; set; } = 0;  // Higher priority wins when stacking

        // Admin Notes
        public string? AdminNotes { get; set; }

        // Computed Properties
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsScheduled => DateTime.UtcNow < StartDate;
        public bool IsCurrentlyActive => !IsExpired && !IsScheduled && Status == OfferStatus.Active;
    }

    public enum OfferType
    {
        Percentage,
        FixedAmount
    }

    public enum OfferTargetType
    {
        Product,
        Category,
        Occasion,
        All
    }

    public enum OfferStatus
    {
        Active,
        Scheduled,
        Expired,
        Disabled
    }
}
