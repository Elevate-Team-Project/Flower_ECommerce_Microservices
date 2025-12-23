namespace Ordering_Service.Features.Orders.CreateOrder
{
    public record CreateOrderDto(
        int OrderId,
        string UserId,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal ShippingCost,
        decimal TotalAmount,
        string Status,
        DateTime CreatedAt
    );
}
