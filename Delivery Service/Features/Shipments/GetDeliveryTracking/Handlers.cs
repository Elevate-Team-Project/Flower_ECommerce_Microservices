using BuildingBlocks.Interfaces;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Delivery_Service.Features.Shipments.GetDeliveryTracking
{
    public class GetDeliveryTrackingHandler : IRequestHandler<GetDeliveryTrackingQuery, EndpointResponse<DeliveryTrackingDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;

        public GetDeliveryTrackingHandler(IBaseRepository<Shipment> shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<EndpointResponse<DeliveryTrackingDto>> Handle(
            GetDeliveryTrackingQuery request,
            CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.Get(s => s.Id == request.ShipmentId)
                .Include(s => s.DeliveryAddress)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (shipment == null)
                return EndpointResponse<DeliveryTrackingDto>.NotFoundResponse("Shipment not found");

            // Calculate ETA based on last location update
            int? estimatedMinutesRemaining = null;
            DateTime? estimatedArrival = null;
            
            if (shipment.EstimatedDeliveryDate.HasValue && shipment.Status == "OutForDelivery")
            {
                estimatedArrival = shipment.EstimatedDeliveryDate;
                var timeRemaining = shipment.EstimatedDeliveryDate.Value - DateTime.UtcNow;
                estimatedMinutesRemaining = Math.Max(0, (int)timeRemaining.TotalMinutes);
            }

            // Build full address string
            string? fullAddress = null;
            if (shipment.DeliveryAddress != null)
            {
                var addr = shipment.DeliveryAddress;
                fullAddress = $"{addr.Street}, {addr.City}, {addr.Governorate}";
            }

            var trackingDto = new DeliveryTrackingDto(
                ShipmentId: shipment.Id,
                OrderId: shipment.OrderId,
                Status: shipment.Status,
                DriverLatitude: shipment.DriverLatitude,
                DriverLongitude: shipment.DriverLongitude,
                LastLocationUpdate: shipment.LastLocationUpdate,
                DestinationLatitude: shipment.DeliveryAddress?.Latitude,
                DestinationLongitude: shipment.DeliveryAddress?.Longitude,
                DeliveryAddress: fullAddress,
                DriverName: shipment.DriverName,
                DriverPhone: shipment.DriverPhone,
                DriverPhotoUrl: shipment.DriverPhotoUrl,
                EstimatedArrival: estimatedArrival,
                EstimatedMinutesRemaining: estimatedMinutesRemaining
            );

            return EndpointResponse<DeliveryTrackingDto>.SuccessResponse(trackingDto);
        }
    }
}
