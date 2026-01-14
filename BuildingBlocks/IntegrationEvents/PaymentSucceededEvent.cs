namespace BuildingBlocks.IntegrationEvents
{
    /// <summary>
    /// Event published when a payment succeeds.
    /// Consumed by Ordering Service to update order status.
    /// </summary>
    public record PaymentSucceededEvent(
        int OrderId,
        string UserId,
        decimal Amount,
        string PaymentIntentId,
        string PaymentMethod,
        DateTime PaidAt
    );
}
