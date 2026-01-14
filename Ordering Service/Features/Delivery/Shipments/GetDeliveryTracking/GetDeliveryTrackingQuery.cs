using MediatR;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Shipments.GetDeliveryTracking
{
    /// <summary>
    /// US-E02: Get delivery tracking info for map display
    /// </summary>
    public record GetDeliveryTrackingQuery(int ShipmentId) : IRequest<EndpointResponse<DeliveryTrackingDto>>;

    public record DeliveryTrackingDto(
        int ShipmentId,
        int OrderId,
        string Status,
        
        // Driver Location
        double? DriverLatitude,
        double? DriverLongitude,
        DateTime? LastLocationUpdate,
        
        // Delivery Address
        double? DestinationLatitude,
        double? DestinationLongitude,
        string? DeliveryAddress,
        
        // Driver Info
        string? DriverName,
        string? DriverPhone,
        string? DriverPhotoUrl,
        
        // ETA
        DateTime? EstimatedArrival,
        int? EstimatedMinutesRemaining
    );
}
