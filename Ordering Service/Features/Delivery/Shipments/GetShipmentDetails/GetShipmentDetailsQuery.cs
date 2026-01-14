using MediatR;
using Ordering_Service.Features.Delivery.Shared;
using Ordering_Service.Features.Delivery.Shipments.CreateShipment;

namespace Ordering_Service.Features.Delivery.Shipments.GetShipmentDetails
{
    public record GetShipmentDetailsQuery(int ShipmentId) : IRequest<EndpointResponse<ShipmentDto>>;
    public record GetShipmentByTrackingQuery(string TrackingNumber) : IRequest<EndpointResponse<ShipmentDto>>;
    public record GetOrderShipmentsQuery(int OrderId) : IRequest<EndpointResponse<List<ShipmentDto>>>;
}
