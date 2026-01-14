using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ReOrder
{
    /// <summary>
    /// Command to reorder an existing delivered order with modifications.
    /// US-E03: ReOrder a delivered order
    /// </summary>
    public record ReOrderCommand(
        string UserId,
        int OriginalOrderId,
        List<ReOrderItemDto> Items,           // Modified items list
        int? DeliveryAddressId = null,        // Optional: new address from saved addresses
        string? ShippingAddress = null,       // Optional: address string if not using saved address
        string? Notes = null
    ) : IRequest<EndpointResponse<ReOrderResponseDto>>;

    /// <summary>
    /// Item in a reorder request. Set Quantity to 0 to remove the item.
    /// </summary>
    public record ReOrderItemDto(
        int ProductId,
        int Quantity                          // New quantity (0 to remove item)
    );

    /// <summary>
    /// Response DTO for a successful reorder operation.
    /// </summary>
    public record ReOrderResponseDto(
        int NewOrderId,
        int OriginalOrderId,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal ShippingCost,
        decimal TotalAmount,
        string Status,
        DateTime CreatedAt,
        string ShippingAddress,
        int? DeliveryAddressId,
        List<ReOrderItemResponseDto> Items
    );

    /// <summary>
    /// Item details in the reorder response.
    /// </summary>
    public record ReOrderItemResponseDto(
        int ProductId,
        string ProductName,
        string? ProductImageUrl,
        decimal UnitPrice,
        int Quantity,
        decimal TotalPrice
    );
}
