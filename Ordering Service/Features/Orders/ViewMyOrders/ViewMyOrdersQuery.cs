using MediatR;
using Ordering_Service.Features.Orders.ViewMyOrders.ViewModels;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ViewMyOrders
{
    public record ViewMyOrdersQuery(string UserId, string? Status)
      : IRequest<RequestResponse<List<OrderSummaryViewModel>>>;
}
