using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.GetOrderDetails
{
    public record GetOrderDetailsQuery(
        int OrderId,
        string UserId
    ) : IRequest<EndpointResponse<OrderDetailDto>>;
}
