using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.CreateOrder
{
    public record CreateOrderCommand(
        string UserId,
        List<CreateOrderItemDto> Items,
        string ShippingAddress,
        string BillingAddress,
        string PaymentMethod,
        string? CouponCode = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<CreateOrderDto>>;

    public record CreateOrderItemDto(
        int ProductId,
        string ProductName,
        string? ProductImageUrl,
        decimal UnitPrice,
        int Quantity
    );
}
