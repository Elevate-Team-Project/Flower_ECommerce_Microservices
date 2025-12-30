using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using MediatR;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Shipments.CreateShipment
{
    public class CreateShipmentHandler : IRequestHandler<CreateShipmentCommand, EndpointResponse<ShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrderingGrpc.OrderingGrpcClient _orderingClient;
        private readonly ILogger<CreateShipmentHandler> _logger;

        public CreateShipmentHandler(
            IBaseRepository<Shipment> shipmentRepository, 
            IUnitOfWork unitOfWork,
            OrderingGrpc.OrderingGrpcClient orderingClient,
            ILogger<CreateShipmentHandler> logger)
        {
            _shipmentRepository = shipmentRepository;
            _unitOfWork = unitOfWork;
            _orderingClient = orderingClient;
            _logger = logger;
        }

        public async Task<EndpointResponse<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
        {
            // Validate order exists via Ordering Service gRPC
            OrderResponse orderResponse;
            try
            {
                orderResponse = await _orderingClient.GetOrderByIdAsync(
                    new GetOrderRequest { OrderId = request.OrderId },
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call Ordering gRPC service");
                return EndpointResponse<ShipmentDto>.ErrorResponse(
                    $"Failed to validate order: {ex.Message}", 
                    503);
            }
            
            if (!orderResponse.Success)
            {
                return EndpointResponse<ShipmentDto>.ErrorResponse(
                    $"Order not found or not accessible: {orderResponse.ErrorMessage}", 
                    404);
            }

            var order = orderResponse.Order;
            
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
