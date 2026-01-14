using BuildingBlocks.Interfaces;
using MediatR;
using Ordering_Service.Entities;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Shipments.CreateShipment
{
    public class CreateShipmentHandler : IRequestHandler<CreateShipmentCommand, EndpointResponse<ShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateShipmentHandler> _logger;

        public CreateShipmentHandler(
            IBaseRepository<Shipment> shipmentRepository, 
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateShipmentHandler> logger)
        {
            _shipmentRepository = shipmentRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
        {
            // Validate order exists directly via Repository
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            
            if (order == null)
            {
                return EndpointResponse<ShipmentDto>.ErrorResponse(
                    $"Order not found with ID {request.OrderId}", 
                    404);
            }

            // Validate order status allows shipment creation
            if (order.Status != "Confirmed" && order.Status != "Paid" && order.Status != "Pending")
            {
                return EndpointResponse<ShipmentDto>.ErrorResponse(
                    $"Cannot create shipment for order with status '{order.Status}'", 
                    400);
            }

            var shipment = new Shipment
            {
                OrderId = request.OrderId,
                DeliveryAddressId = request.DeliveryAddressId,
                TrackingNumber = request.TrackingNumber,
                Carrier = request.Carrier,
                Status = "Pending",
                EstimatedDeliveryDate = request.EstimatedDeliveryDate,
                IsGift = request.IsGift,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                GiftMessage = request.GiftMessage,
                Notes = request.Notes
            };

            await _shipmentRepository.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new ShipmentDto(
                shipment.Id, shipment.OrderId, shipment.DeliveryAddressId,
                shipment.TrackingNumber, shipment.Carrier, shipment.Status,
                shipment.EstimatedDeliveryDate, shipment.ActualDeliveryDate,
                shipment.CurrentLocation, shipment.IsGift, shipment.RecipientName,
                shipment.GiftMessage, shipment.CreatedAt
            );

            return EndpointResponse<ShipmentDto>.SuccessResponse(dto, "Shipment created successfully", 201);
        }
    }
}

