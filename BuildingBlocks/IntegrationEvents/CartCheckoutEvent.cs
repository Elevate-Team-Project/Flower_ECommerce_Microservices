using System;
using System.Collections.Generic;

namespace BuildingBlocks.IntegrationEvents
{
    public class CartCheckoutEvent
    {
        public string UserId { get; set; } = string.Empty;
        public List<CartCheckoutItemDto> Items { get; set; } = new();
        public int? DeliveryAddressId { get; set; }
        public string? ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CouponCode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsGift { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? GiftMessage { get; set; }
        public int PointsRedeemed { get; set; }
    }

    public class CartCheckoutItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
