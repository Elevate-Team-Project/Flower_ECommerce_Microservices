using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.GetUserOrders
{
    public record GetUserOrdersQuery(
        string UserId,
        int Page = 1,
        int PageSize = 10,
        string? Status = null
    ) : IRequest<EndpointResponse<PaginatedResult<OrderSummaryDto>>>;
}
