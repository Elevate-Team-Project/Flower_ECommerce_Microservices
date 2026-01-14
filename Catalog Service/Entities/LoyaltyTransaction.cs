using BuildingBlocks.SharedEntities;

namespace Catalog_Service.Entities
{
    /// <summary>
    /// Tracks individual loyalty point transactions (earn/redeem).
    /// US-H01, US-H02, US-H03: Loyalty Points History
    /// </summary>
    public class LoyaltyTransaction : BaseEntity
    {
        public int LoyaltyAccountId { get; set; }
        public TransactionType Type { get; set; }
        public int Points { get; set; }  // Positive for earn, can store as positive and use Type
        public string Description { get; set; } = string.Empty;

        // Reference to source
        public int? OrderId { get; set; }
        public decimal? OrderAmount { get; set; }

        // Balance after transaction
        public int BalanceAfter { get; set; }

        // Navigation
        public virtual LoyaltyAccount Account { get; set; } = null!;
    }

    public enum TransactionType
    {
        Earned,
        Redeemed,
        Expired,
        Adjustment,  // Manual adjustment by admin
        Bonus        // Birthday, tier upgrade, promotion, etc.
    }
}
