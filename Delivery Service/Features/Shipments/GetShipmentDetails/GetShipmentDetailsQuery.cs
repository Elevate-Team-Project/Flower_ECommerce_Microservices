using MediatR;
using Delivery_Service.Features.Shared;
using Delivery_Service.Features.Shipments.CreateShipment;

namespace Delivery_Service.Features.Shipments.GetShipmentDetails
{
    public record GetShipmentDetailsQuery(int ShipmentId) : IRequest<EndpointResponse<ShipmentDto>>;
    public record GetShipmentByTrackingQuery(string TrackingNumber) : IRequest<EndpointResponse<ShipmentDto>>;
    public record GetOrderShipmentsQuery(int OrderId) : IRequest<EndpointResponse<List<ShipmentDto>>>;
}
