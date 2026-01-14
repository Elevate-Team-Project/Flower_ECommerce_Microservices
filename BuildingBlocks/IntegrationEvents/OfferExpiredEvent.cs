namespace BuildingBlocks.IntegrationEvents
{
    /// <summary>
    /// Event published when an offer expires.
    /// Consumed by Catalog Service to reset product discounted prices.
    /// </summary>
    public record OfferExpiredEvent(
        int OfferId,
        string OfferName,
        OfferExpiredTargetType TargetType,
        int? ProductId,
        int? CategoryId,
        int? OccasionId,
        DateTime ExpiredAt
    );

    public enum OfferExpiredTargetType
    {
        Product,
        Category,
        Occasion,
        All
    }
}
