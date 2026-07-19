using System;
using System.Collections.Generic;

namespace BuildingBlocks.IntegrationEvents
{
    public class CodOrderApprovedEvent
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int DeliveryAddressId { get; set; }
        public bool IsGift { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? GiftMessage { get; set; }
        public List<CodOrderApprovedItemDto> Items { get; set; } = new();
    }

    public class CodOrderApprovedItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
