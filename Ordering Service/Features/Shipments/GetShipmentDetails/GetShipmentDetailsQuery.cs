using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Shipments.GetShipmentDetails
{
    public record GetShipmentDetailsQuery(
        int ShipmentId
    ) : IRequest<EndpointResponse<ShipmentDto>>;
}
