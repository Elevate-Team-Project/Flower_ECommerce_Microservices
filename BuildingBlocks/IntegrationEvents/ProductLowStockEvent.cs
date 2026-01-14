namespace BuildingBlocks.IntegrationEvents
{
    public record ProductLowStockEvent(
        int ProductId,
        string ProductName,
        int CurrentStock,
        int MinStock
    );
}
