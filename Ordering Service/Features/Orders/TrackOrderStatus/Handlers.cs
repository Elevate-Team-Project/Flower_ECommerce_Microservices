using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders
{
    public class GetOrdersStatusHandler : IRequestHandler<GetOrdersStatusQuery, EndpointResponse<PaginatedResult<OrderStatusHistoryDto>>>
    {
        private readonly IBaseRepository<Order> _orderRepository;

        public GetOrdersStatusHandler(IBaseRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<EndpointResponse<PaginatedResult<OrderStatusHistoryDto>>> Handle(
            GetOrdersStatusQuery request,
            CancellationToken cancellationToken)
        {
            var query = _orderRepository.GetAll()
              .Where(x => x.Id == request.OrderId);

         

            var totalCount = await query.CountAsync(cancellationToken);

            var data = await query
            .Select(x => new OrderStatusHistoryDto
            {
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<OrderStatusHistoryDto>(
              data,
                totalCount,
                request.Page,
                request.PageSize
            );

            return EndpointResponse<PaginatedResult<OrderStatusHistoryDto>>.SuccessResponse(
                paginatedResult,
                "User orders retrieved successfully"
            );
        }
    }
}
