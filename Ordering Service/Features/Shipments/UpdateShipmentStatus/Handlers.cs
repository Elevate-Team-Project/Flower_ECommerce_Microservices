using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Shipments.UpdateShipmentStatus
{
    public class UpdateShipmentStatusHandler : IRequestHandler<UpdateShipmentStatusCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShipmentStatusHandler(
            IBaseRepository<Shipment> shipmentRepository,
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<bool>> Handle(
            UpdateShipmentStatusCommand request,
            CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAll()
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

            if (shipment == null)
            {
                return EndpointResponse<bool>.NotFoundResponse("Shipment not found");
            }

            // Validate status transition
            var validStatuses = new[] { "Pending", "InTransit", "OutForDelivery", "Delivered", "Failed" };
            if (!validStatuses.Contains(request.NewStatus))
            {
                return EndpointResponse<bool>.ErrorResponse($"Invalid status. Valid statuses: {string.Join(", ", validStatuses)}");
            }

            shipment.Status = request.NewStatus;

            if (!string.IsNullOrEmpty(request.CurrentLocation))
            {
                shipment.CurrentLocation = request.CurrentLocation;
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                shipment.Notes = request.Notes;
            }

            // Handle delivery completion
            if (request.NewStatus == "Delivered")
            {
                shipment.ActualDeliveryDate = DateTime.UtcNow;

                // Update order status to Delivered
                var order = shipment.Order;
                if (order != null && order.Status != "Delivered")
                {
                    order.Status = "Delivered";
                    order.DeliveredAt = DateTime.UtcNow;
                    _orderRepository.Update(order);
                }
            }

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, $"Shipment status updated to {request.NewStatus}");
        }
    }
}
