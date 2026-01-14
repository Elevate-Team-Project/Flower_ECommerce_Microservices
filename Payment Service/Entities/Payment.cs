using BuildingBlocks.SharedEntities;

namespace Payment_Service.Entities
{
    /// <summary>
    /// Represents a payment transaction record.
    /// </summary>
    public class Payment : BaseEntity
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        
        // Stripe-specific
        public string? StripePaymentIntentId { get; set; }
        public string? StripeClientSecret { get; set; }
        
        // Payment details
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string PaymentMethod { get; set; } = string.Empty; // "CreditCard", "CashOnDelivery"
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        
        // Timestamps
        public DateTime? PaidAt { get; set; }
        public DateTime? FailedAt { get; set; }
        
        // Error handling
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        
        // Refund tracking
        public bool IsRefunded { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed,
        Cancelled,
        Refunded
    }
}
