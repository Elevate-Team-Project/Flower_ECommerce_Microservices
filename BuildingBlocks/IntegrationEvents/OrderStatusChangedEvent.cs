namespace BuildingBlocks.IntegrationEvents
{
    /// <summary>
    /// Published when an order status changes
    /// </summary>
    public class OrderStatusChangedEvent
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
