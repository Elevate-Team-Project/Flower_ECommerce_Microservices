namespace Ordering_Service.Features.Orders.GetUserOrders
{
    public record OrderSummaryDto(
        int OrderId,
        decimal TotalAmount,
        string Status,
        int ItemCount,
        DateTime CreatedAt,
        DateTime? DeliveredAt
    );
}
