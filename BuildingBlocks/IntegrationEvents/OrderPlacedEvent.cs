using System;
using System.Collections.Generic;

namespace BuildingBlocks.IntegrationEvents
{
    public class OrderPlacedEvent
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int DeliveryAddressId { get; set; }
        public bool IsGift { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? GiftMessage { get; set; }
        public List<OrderPlacedItemDto> Items { get; set; } = new();
    }

    public class OrderPlacedItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
