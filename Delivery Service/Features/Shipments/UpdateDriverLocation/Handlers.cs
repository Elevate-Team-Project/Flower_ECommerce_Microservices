using BuildingBlocks.Interfaces;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;
using MediatR;

namespace Delivery_Service.Features.Shipments.UpdateDriverLocation
{
    public class UpdateDriverLocationHandler : IRequestHandler<UpdateDriverLocationCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateDriverLocationHandler> _logger;

        public UpdateDriverLocationHandler(
            IBaseRepository<Shipment> shipmentRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateDriverLocationHandler> logger)
        {
            _shipmentRepository = shipmentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<bool>> Handle(
            UpdateDriverLocationCommand request,
            CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            
            if (shipment == null)
                return EndpointResponse<bool>.NotFoundResponse("Shipment not found");

            if (shipment.Status != "OutForDelivery" && shipment.Status != "InTransit")
                return EndpointResponse<bool>.ErrorResponse("Location can only be updated for shipments in transit or out for delivery");

            // Update location
            shipment.DriverLatitude = request.Latitude;
            shipment.DriverLongitude = request.Longitude;
            shipment.LastLocationUpdate = DateTime.UtcNow;

            // Update driver info if provided
            if (!string.IsNullOrEmpty(request.DriverName))
                shipment.DriverName = request.DriverName;
            if (!string.IsNullOrEmpty(request.DriverPhone))
                shipment.DriverPhone = request.DriverPhone;

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated location for shipment {ShipmentId}: ({Lat}, {Long})",
                request.ShipmentId, request.Latitude, request.Longitude);

            return EndpointResponse<bool>.SuccessResponse(true, "Location updated successfully");
        }
    }
}
