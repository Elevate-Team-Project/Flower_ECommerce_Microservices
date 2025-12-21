using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Shipments.CreateShipment
{
    public record CreateShipmentCommand(
        int OrderId,
        string TrackingNumber,
        string Carrier,
        DateTime? EstimatedDeliveryDate = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<CreateShipmentDto>>;

    public record CreateShipmentDto(
        int ShipmentId,
        int OrderId,
        string TrackingNumber,
        string Carrier,
        string Status,
        DateTime? EstimatedDeliveryDate,
        DateTime CreatedAt
    );
}
