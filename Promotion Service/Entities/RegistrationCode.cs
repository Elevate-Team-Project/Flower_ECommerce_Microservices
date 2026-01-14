using BuildingBlocks.SharedEntities;

namespace Promotion_Service.Entities
{
    /// <summary>
    /// Registration codes for new user benefits (welcome credits, group upgrades).
    /// </summary>
    public class RegistrationCode : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Code Type
        public RegistrationCodeType Type { get; set; }

        // Benefits
        public int? TargetCustomerGroupId { get; set; }  // For group upgrade
        public decimal? WelcomeCreditAmount { get; set; }  // For wallet credit
        public string Currency { get; set; } = "EGP";

        // Usage Limits
        public int? MaxTotalUsage { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public int CurrentUsageCount { get; set; } = 0;

        // Validity
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        // Admin Notes
        public string? AdminNotes { get; set; }

        // Navigation
        public virtual ICollection<RegistrationCodeUsage> Usages { get; set; } = new List<RegistrationCodeUsage>();

        // Computed
        public bool IsExpired => DateTime.UtcNow > ValidUntil;
        public bool HasReachedMaxUsage => MaxTotalUsage.HasValue && CurrentUsageCount >= MaxTotalUsage.Value;
    }

    public enum RegistrationCodeType
    {
        GroupUpgrade,
        WalletCredit,
        Both
    }
}
