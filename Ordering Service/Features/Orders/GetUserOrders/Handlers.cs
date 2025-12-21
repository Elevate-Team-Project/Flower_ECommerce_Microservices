using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.GetUserOrders
{
    public class GetUserOrdersHandler : IRequestHandler<GetUserOrdersQuery, EndpointResponse<PaginatedResult<OrderSummaryDto>>>
    {
        private readonly IBaseRepository<Order> _orderRepository;

        public GetUserOrdersHandler(IBaseRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<EndpointResponse<PaginatedResult<OrderSummaryDto>>> Handle(
            GetUserOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _orderRepository.GetAll()
                .Include(o => o.Items)
                .Where(o => o.UserId == request.UserId);

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(o => o.Status == request.Status);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OrderSummaryDto(
                    o.Id,
                    o.TotalAmount,
                    o.Status,
                    o.Items.Count,
                    o.CreatedAt,
                    o.DeliveredAt
                ))
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<OrderSummaryDto>(
                orders,
                totalCount,
                request.Page,
                request.PageSize
            );

            return EndpointResponse<PaginatedResult<OrderSummaryDto>>.SuccessResponse(
                paginatedResult,
                "User orders retrieved successfully"
            );
        }
    }
}
