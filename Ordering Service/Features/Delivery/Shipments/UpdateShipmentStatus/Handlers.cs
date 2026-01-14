using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Shipments.UpdateShipmentStatus
{
    public class UpdateShipmentStatusHandler : IRequestHandler<UpdateShipmentStatusCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShipmentStatusHandler(IBaseRepository<Shipment> shipmentRepository, IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<bool>> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

            if (shipment == null)
                return EndpointResponse<bool>.NotFoundResponse("Shipment not found");

            var validStatuses = new[] { "Pending", "InTransit", "OutForDelivery", "Delivered", "Failed" };
            if (!validStatuses.Contains(request.NewStatus))
                return EndpointResponse<bool>.ErrorResponse($"Invalid status. Valid: {string.Join(", ", validStatuses)}");

            shipment.Status = request.NewStatus;
            if (!string.IsNullOrEmpty(request.CurrentLocation)) shipment.CurrentLocation = request.CurrentLocation;
            if (!string.IsNullOrEmpty(request.Notes)) shipment.Notes = request.Notes;
            if (request.NewStatus == "Delivered") shipment.ActualDeliveryDate = DateTime.UtcNow;

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, $"Shipment status updated to {request.NewStatus}");
        }
    }
}
