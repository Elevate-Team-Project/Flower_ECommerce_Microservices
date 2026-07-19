using System;
using System.Collections.Generic;

namespace BuildingBlocks.IntegrationEvents
{
    /// <summary>
    /// Event published when a payment succeeds.
    /// Consumed by Ordering and Delivery Services.
    /// </summary>
    public record PaymentSucceededEvent(
        int OrderId,
        string UserId,
        decimal Amount,
        string PaymentIntentId,
        string PaymentMethod,
        DateTime PaidAt,
        int DeliveryAddressId,
        bool IsGift,
        string? RecipientName,
        string? RecipientPhone,
        string? GiftMessage,
        List<PaymentSucceededItemDto> Items
    );

    public record PaymentSucceededItemDto(
        int ProductId,
        int Quantity
    );
}
