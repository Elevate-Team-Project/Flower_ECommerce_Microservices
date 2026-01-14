using BuildingBlocks.SharedEntities;

namespace Catalog_Service.Entities
{
    /// <summary>
    /// Represents a user's loyalty points account.
    /// US-H01, US-H02: Loyalty Points System
    /// </summary>
    public class LoyaltyAccount : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int CurrentPoints { get; set; } = 0;
        public int TotalEarnedPoints { get; set; } = 0;
        public int TotalRedeemedPoints { get; set; } = 0;

        // Tier Information
        public int TierId { get; set; } = 1;  // Default to lowest tier

        // Navigation
        public virtual LoyaltyTier Tier { get; set; } = null!;
        public virtual ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }
}
