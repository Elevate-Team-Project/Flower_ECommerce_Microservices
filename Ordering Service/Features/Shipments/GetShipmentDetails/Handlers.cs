using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Shipments.GetShipmentDetails
{
    public class GetShipmentDetailsHandler : IRequestHandler<GetShipmentDetailsQuery, EndpointResponse<ShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;

        public GetShipmentDetailsHandler(IBaseRepository<Shipment> shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<EndpointResponse<ShipmentDto>> Handle(
            GetShipmentDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

            if (shipment == null)
            {
                return EndpointResponse<ShipmentDto>.NotFoundResponse("Shipment not found");
            }

            var dto = new ShipmentDto(
                shipment.Id,
                shipment.OrderId,
                shipment.TrackingNumber,
                shipment.Carrier,
                shipment.Status,
                shipment.EstimatedDeliveryDate,
                shipment.ActualDeliveryDate,
                shipment.CurrentLocation,
                shipment.Notes,
                shipment.CreatedAt,
                shipment.UpdatedAt
            );

            return EndpointResponse<ShipmentDto>.SuccessResponse(dto, "Shipment details retrieved successfully");
        }
    }

    public class GetShipmentByTrackingHandler : IRequestHandler<GetShipmentByTrackingQuery, EndpointResponse<ShipmentDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;

        public GetShipmentByTrackingHandler(IBaseRepository<Shipment> shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<EndpointResponse<ShipmentDto>> Handle(
            GetShipmentByTrackingQuery request,
            CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAll()
                .FirstOrDefaultAsync(s => s.TrackingNumber == request.TrackingNumber, cancellationToken);

            if (shipment == null)
            {
                return EndpointResponse<ShipmentDto>.NotFoundResponse("Shipment not found");
            }

            var dto = new ShipmentDto(
                shipment.Id,
                shipment.OrderId,
                shipment.TrackingNumber,
                shipment.Carrier,
                shipment.Status,
                shipment.EstimatedDeliveryDate,
                shipment.ActualDeliveryDate,
                shipment.CurrentLocation,
                shipment.Notes,
                shipment.CreatedAt,
                shipment.UpdatedAt
            );

            return EndpointResponse<ShipmentDto>.SuccessResponse(dto, "Shipment details retrieved successfully");
        }
    }
}
