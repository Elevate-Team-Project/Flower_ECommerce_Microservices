using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Shipments.UpdateShipmentStatus
{
    public record UpdateShipmentStatusCommand(
        int ShipmentId,
        string NewStatus,
        string? CurrentLocation = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<bool>>;
}
