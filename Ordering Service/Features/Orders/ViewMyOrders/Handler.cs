using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Orders.ViewMyOrders.ViewModels;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ViewMyOrders
{
    public class Handler : IRequestHandler<ViewMyOrdersQuery, RequestResponse<List<OrderSummaryViewModel>>>
    {
        private readonly IBaseRepository<Order> _orderRepo;

        public Handler(IBaseRepository<Order> orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<RequestResponse<List<OrderSummaryViewModel>>> Handle(
            ViewMyOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _orderRepo
                .Get(o => o.UserId == request.UserId)
                .OrderByDescending(o => o.CreatedAt);

            var allowed = new[] { "active", "completed", "cancelled" };

            if (!string.IsNullOrEmpty(request.Status) &&
                !allowed.Contains(request.Status.ToLower()))
            {
                return RequestResponse<List<OrderSummaryViewModel>>
                    .Fail("Status must be one of: active, completed, cancelled");
            }

            if (request.Status?.ToLower() == "active")
            {
                query = (IOrderedQueryable<Order>)query.Where(o =>
                    o.Status != "Delivered" &&
                    o.Status != "Cancelled");
            }

            if (request.Status?.ToLower() == "completed")
            {
                query = (IOrderedQueryable<Order>)query.Where(o =>
                    o.Status == "Delivered");
            }

            if (request.Status?.ToLower() == "cancelled")
            {
                query = (IOrderedQueryable<Order>)query.Where(o =>
                    o.Status == "Cancelled");
            }

            var orders = await query
                .Select(o => new OrderSummaryViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.Id.ToString(),
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    DeliveredAt = o.DeliveredAt,
                    TotalAmount = o.TotalAmount,
                    Items = o.Items.Select(i => new OrderItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        ProductImageUrl = i.ProductImageUrl
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return RequestResponse<List<OrderSummaryViewModel>>
                .Success(orders, "Orders fetched successfully");
        }
    }
}
