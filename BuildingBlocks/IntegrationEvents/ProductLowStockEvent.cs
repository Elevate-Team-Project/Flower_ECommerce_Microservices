namespace BuildingBlocks.IntegrationEvents
{
    public record ProductLowStockEvent
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty; // قيمة مبدئية لتجنب الـ Null
        public int CurrentStock { get; init; }
        public int MinStock { get; init; }
    }
}
