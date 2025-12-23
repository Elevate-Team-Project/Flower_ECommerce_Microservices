using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;
using Delivery_Service.Features.Shipments.CreateShipment;

namespace Delivery_Service.Features.Shipments.GetShipmentDetails
{
    public class GetShipmentDetailsHandler : IRequestHandler<GetShipmentDetailsQuery, EndpointResponse<ShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        public GetShipmentDetailsHandler(IBaseRepository<Shipment> shipmentRepository) => _shipmentRepository = shipmentRepository;

        public async Task<EndpointResponse<ShipmentDto>> Handle(GetShipmentDetailsQuery request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAll().FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);
            if (shipment == null) return EndpointResponse<ShipmentDto>.NotFoundResponse("Shipment not found");

            return EndpointResponse<ShipmentDto>.SuccessResponse(MapToDto(shipment));
        }

        private static ShipmentDto MapToDto(Shipment s) => new(
            s.Id, s.OrderId, s.DeliveryAddressId, s.TrackingNumber, s.Carrier, s.Status,
            s.EstimatedDeliveryDate, s.ActualDeliveryDate, s.CurrentLocation, s.IsGift,
            s.RecipientName, s.GiftMessage, s.CreatedAt
        );
    }

    public class GetShipmentByTrackingHandler : IRequestHandler<GetShipmentByTrackingQuery, EndpointResponse<ShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        public GetShipmentByTrackingHandler(IBaseRepository<Shipment> shipmentRepository) => _shipmentRepository = shipmentRepository;

        public async Task<EndpointResponse<ShipmentDto>> Handle(GetShipmentByTrackingQuery request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAll().FirstOrDefaultAsync(s => s.TrackingNumber == request.TrackingNumber, cancellationToken);
            if (shipment == null) return EndpointResponse<ShipmentDto>.NotFoundResponse("Shipment not found");

            return EndpointResponse<ShipmentDto>.SuccessResponse(new ShipmentDto(
                shipment.Id, shipment.OrderId, shipment.DeliveryAddressId, shipment.TrackingNumber,
                shipment.Carrier, shipment.Status, shipment.EstimatedDeliveryDate, shipment.ActualDeliveryDate,
                shipment.CurrentLocation, shipment.IsGift, shipment.RecipientName, shipment.GiftMessage, shipment.CreatedAt
            ));
        }
    }

    public class GetOrderShipmentsHandler : IRequestHandler<GetOrderShipmentsQuery, EndpointResponse<List<ShipmentDto>>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        public GetOrderShipmentsHandler(IBaseRepository<Shipment> shipmentRepository) => _shipmentRepository = shipmentRepository;

        public async Task<EndpointResponse<List<ShipmentDto>>> Handle(GetOrderShipmentsQuery request, CancellationToken cancellationToken)
        {
            var shipments = await _shipmentRepository.GetAll()
                .Where(s => s.OrderId == request.OrderId)
                .Select(s => new ShipmentDto(
                    s.Id, s.OrderId, s.DeliveryAddressId, s.TrackingNumber, s.Carrier, s.Status,
                    s.EstimatedDeliveryDate, s.ActualDeliveryDate, s.CurrentLocation, s.IsGift,
                    s.RecipientName, s.GiftMessage, s.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<ShipmentDto>>.SuccessResponse(shipments);
        }
    }
}
