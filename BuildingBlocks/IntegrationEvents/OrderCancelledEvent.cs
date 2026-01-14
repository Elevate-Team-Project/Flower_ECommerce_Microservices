namespace BuildingBlocks.IntegrationEvents
{
    /// <summary>
    /// Event published when an order is cancelled.
    /// Consumed by Catalog Service to restore reserved stock.
    /// </summary>
    public record OrderCancelledEvent(
        int OrderId,
        string UserId,
        string CancellationReason,
        DateTime CancelledAt,
        List<OrderCancelledItemDto> Items
    );

    public record OrderCancelledItemDto(
        int ProductId,
        int Quantity
    );
}
