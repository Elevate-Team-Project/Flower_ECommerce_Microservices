using BuildingBlocks.SharedEntities;

namespace Promotion_Service.Entities
{
    /// <summary>
    /// Tracks registration code usage.
    /// </summary>
    public class RegistrationCodeUsage : BaseEntity
    {
        public int RegistrationCodeId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        // Applied Benefits
        public bool GroupUpgradeApplied { get; set; }
        public bool WalletCreditApplied { get; set; }
        public decimal? CreditAmount { get; set; }

        // Navigation
        public virtual RegistrationCode RegistrationCode { get; set; } = null!;
    }
}
