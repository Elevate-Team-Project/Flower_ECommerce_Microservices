using BuildingBlocks.Interfaces;
using BuildingBlocks.ServiceClients;
using MediatR;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Shipments.CreateShipment
{
    public class CreateShipmentHandler : IRequestHandler<CreateShipmentCommand, EndpointResponse<ShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderingServiceClient _orderingClient;

        public CreateShipmentHandler(
            IBaseRepository<Shipment> shipmentRepository, 
            IUnitOfWork unitOfWork,
            IOrderingServiceClient orderingClient)
        {
            _shipmentRepository = shipmentRepository;
            _unitOfWork = unitOfWork;
            _orderingClient = orderingClient;
        }

        public async Task<EndpointResponse<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
        {
            // Validate order exists via Ordering Service
            var orderResponse = await _orderingClient.GetOrderByIdAsync(request.OrderId, cancellationToken);
            
            if (!orderResponse.IsSuccess)
            {
                return EndpointResponse<ShipmentDto>.ErrorResponse(
                    $"Order not found or not accessible: {orderResponse.ErrorMessage}", 
                    orderResponse.StatusCode);
            }

            var order = orderResponse.Data!;
            
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

