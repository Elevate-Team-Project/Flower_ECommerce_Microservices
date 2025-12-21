namespace Ordering_Service.Features.Shipments.GetShipmentDetails
{
    public record ShipmentDto(
        int ShipmentId,
        int OrderId,
        string TrackingNumber,
        string Carrier,
        string Status,
        DateTime? EstimatedDeliveryDate,
        DateTime? ActualDeliveryDate,
        string? CurrentLocation,
        string? Notes,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
