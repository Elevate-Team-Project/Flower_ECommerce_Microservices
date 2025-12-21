using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Shipments.CreateShipment
{
    public class CreateShipmentHandler : IRequestHandler<CreateShipmentCommand, EndpointResponse<CreateShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateShipmentHandler(
            IBaseRepository<Shipment> shipmentRepository,
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<CreateShipmentDto>> Handle(
            CreateShipmentCommand request,
            CancellationToken cancellationToken)
        {
            // Verify order exists
            var order = await _orderRepository.GetAll()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                return EndpointResponse<CreateShipmentDto>.NotFoundResponse("Order not found");
            }

            var shipment = new Shipment
            {
                OrderId = request.OrderId,
                TrackingNumber = request.TrackingNumber,
                Carrier = request.Carrier,
                Status = "Pending",
                EstimatedDeliveryDate = request.EstimatedDeliveryDate,
                Notes = request.Notes
            };

            await _shipmentRepository.AddAsync(shipment);

            // Update order status to Shipped if not already
            if (order.Status != "Shipped" && order.Status != "Delivered")
            {
                order.Status = "Shipped";
                order.ShippedAt = DateTime.UtcNow;
                _orderRepository.Update(order);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new CreateShipmentDto(
                shipment.Id,
                shipment.OrderId,
                shipment.TrackingNumber,
                shipment.Carrier,
                shipment.Status,
                shipment.EstimatedDeliveryDate,
                shipment.CreatedAt
            );

            return EndpointResponse<CreateShipmentDto>.SuccessResponse(dto, "Shipment created successfully", 201);
        }
    }
}
