using BuildingBlocks.SharedEntities;

namespace Ordering_Service.Entities
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string? CouponCode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; 
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentTransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? Notes { get; set; }

        // Delivery Address Reference (from Delivery Service)
        public int? DeliveryAddressId { get; set; }

        // Gift Order Fields (US-D05)
        public bool IsGift { get; set; } = false;
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? GiftMessage { get; set; }

        // Navigation Properties
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual DiscountUsage? DiscountUsage { get; set; }
        // Note: Shipments are now managed by the Delivery Service
    }
}
