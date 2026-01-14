using MediatR;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Shipments.UpdateShipmentStatus
{
    public record UpdateShipmentStatusCommand(
        int ShipmentId,
        string NewStatus,
        string? CurrentLocation = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<bool>>;
}
