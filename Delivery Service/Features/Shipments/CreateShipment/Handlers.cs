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

        public CreateShipmentHandler(IBaseRepository<Shipment> shipmentRepository, IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
        {
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
