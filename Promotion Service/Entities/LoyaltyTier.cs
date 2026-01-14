using BuildingBlocks.SharedEntities;

namespace Promotion_Service.Entities
{
    /// <summary>
    /// Represents a loyalty tier level (e.g., Silver, Gold, Platinum).
    /// US-H04: Loyalty Tier System
    /// </summary>
    public class LoyaltyTier : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public string? Description { get; set; }

        // Requirements
        public int MinPoints { get; set; }  // Points needed to reach this tier

        // Benefits
        public decimal PointsMultiplier { get; set; } = 1.0m;  // e.g., 1.5x for Gold
        public decimal? DiscountPercentage { get; set; }  // Additional discount for tier members
        public bool FreeShipping { get; set; } = false;
        public int? BonusPointsOnBirthday { get; set; }

        // Display
        public string? IconUrl { get; set; }
        public string? BadgeColor { get; set; }
        public int SortOrder { get; set; }

        // Navigation
        public virtual ICollection<LoyaltyAccount> Accounts { get; set; } = new List<LoyaltyAccount>();
    }
}
