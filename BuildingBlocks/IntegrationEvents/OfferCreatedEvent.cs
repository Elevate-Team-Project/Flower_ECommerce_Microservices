namespace BuildingBlocks.IntegrationEvents
{
    /// <summary>
    /// Published when a new offer is created
    /// </summary>
    public class OfferCreatedEvent
    {
        public int OfferId { get; set; }
        public string OfferName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
