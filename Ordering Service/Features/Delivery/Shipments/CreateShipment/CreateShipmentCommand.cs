using MediatR;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Shipments.CreateShipment
{
    public record CreateShipmentCommand(
        int OrderId,
        int DeliveryAddressId,
        string TrackingNumber,
        string Carrier,
        DateTime? EstimatedDeliveryDate = null,
        bool IsGift = false,
        string? RecipientName = null,
        string? RecipientPhone = null,
        string? GiftMessage = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<ShipmentDto>>;

    public record ShipmentDto(
        int Id,
        int OrderId,
        int DeliveryAddressId,
        string TrackingNumber,
        string Carrier,
        string Status,
        DateTime? EstimatedDeliveryDate,
        DateTime? ActualDeliveryDate,
        string? CurrentLocation,
        bool IsGift,
        string? RecipientName,
        string? GiftMessage,
        DateTime CreatedAt
    );
}
