using MediatR;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Shipments.UpdateShipmentStatus
{
    public record UpdateShipmentStatusCommand(
        int ShipmentId,
        string NewStatus,
        string? CurrentLocation = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<bool>>;
}
