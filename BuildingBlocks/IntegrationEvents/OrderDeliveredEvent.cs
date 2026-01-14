namespace BuildingBlocks.IntegrationEvents
{
    public record OrderDeliveredEvent(
        int OrderId,
        string UserId,
        decimal OrderTotal,
        DateTime DeliveredAt
    );
}
