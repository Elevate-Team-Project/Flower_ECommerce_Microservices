using MediatR;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Shipments.UpdateDriverLocation
{
    /// <summary>
    /// US-E02: Update driver location for real-time tracking
    /// </summary>
    public record UpdateDriverLocationCommand(
        int ShipmentId,
        double Latitude,
        double Longitude,
        string? DriverName = null,
        string? DriverPhone = null
    ) : IRequest<EndpointResponse<bool>>;
}
