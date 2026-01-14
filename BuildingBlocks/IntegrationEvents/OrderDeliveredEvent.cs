namespace BuildingBlocks.IntegrationEvents
{
    public record OrderDeliveredEvent(
        int OrderId,
        string UserId,
        decimal OrderTotal,
        DateTime DeliveredAt,
        List<OrderDeliveredItemDto> Items
    );

    public record OrderDeliveredItemDto(
        int ProductId,
        int Quantity
    );
}
