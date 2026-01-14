using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Cart.Checkout
{
    /// <summary>
    /// US-D07: Confirm Order Placement
    /// Converts cart items to an order
    /// </summary>
    public record CheckoutCommand(
        string UserId,
        int? DeliveryAddressId,
        string? ShippingAddress,
        string PaymentMethod,
        string? Notes,
        string? CouponCode,
        // Gift order fields (US-D05)
        bool IsGift = false,
        string? RecipientName = null,
        string? RecipientPhone = null,
        string? GiftMessage = null,
        int? PointsToRedeem = null
    ) : IRequest<EndpointResponse<CheckoutResultDto>>;

    public record CheckoutResultDto(
        int OrderId,
        string OrderNumber,
        decimal SubTotal,
        decimal DeliveryFee,
        decimal DiscountAmount,
        decimal TotalAmount,
        string Status,
        DateTime EstimatedDelivery,
        // Gift order info (US-D05)
        bool IsGift = false,
        string? RecipientName = null,
        string? GiftMessage = null
    );

    public record CartItemForCheckout(
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        string? ImageUrl
    );
}
