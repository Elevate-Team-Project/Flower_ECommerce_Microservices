namespace Ordering_Service.Features.Orders.GetOrderDetails
{
    public record OrderDetailDto(
        int OrderId,
        string UserId,
        string? CouponCode,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal ShippingCost,
        decimal TotalAmount,
        string Status,
        string ShippingAddress,
        string BillingAddress,
        string PaymentMethod,
        string? PaymentTransactionId,
        DateTime? PaidAt,
        DateTime? ShippedAt,
        DateTime? DeliveredAt,
        string? Notes,
        DateTime CreatedAt,
        List<OrderItemDetailDto> Items,
        List<ShipmentDetailDto> Shipments
    );

    public record OrderItemDetailDto(
        int ItemId,
        int ProductId,
        string ProductName,
        string? ProductImageUrl,
        decimal UnitPrice,
        int Quantity,
        decimal TotalPrice
    );

    public record ShipmentDetailDto(
        int ShipmentId,
        string TrackingNumber,
        string Carrier,
        string Status,
        DateTime? EstimatedDeliveryDate,
        DateTime? ActualDeliveryDate,
        string? CurrentLocation
    );
}
