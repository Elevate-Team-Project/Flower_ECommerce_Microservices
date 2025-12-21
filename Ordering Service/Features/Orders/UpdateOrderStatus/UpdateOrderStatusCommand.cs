using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.UpdateOrderStatus
{
    public record UpdateOrderStatusCommand(
        int OrderId,
        string NewStatus,
        string? Notes = null
    ) : IRequest<EndpointResponse<bool>>;
}
