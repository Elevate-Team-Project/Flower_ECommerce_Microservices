using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;

namespace Ordering_Service.GrpcServices;

/// <summary>
/// gRPC service implementation for Ordering operations.
/// Provides high-performance inter-service communication for order queries.
/// </summary>
public class OrderingGrpcService : OrderingGrpc.OrderingGrpcBase
{
    private readonly IBaseRepository<Order> _orderRepository;
    private readonly ILogger<OrderingGrpcService> _logger;

    public OrderingGrpcService(
        IBaseRepository<Order> orderRepository,
        ILogger<OrderingGrpcService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public override async Task<OrderResponse> GetOrderById(
        GetOrderRequest request,
        ServerCallContext context)
    {
        try
        {
            var order = await _orderRepository
                .GetAll()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, context.CancellationToken);

            if (order == null)
            {
                return new OrderResponse
                {
                    Success = false,
                    ErrorMessage = $"Order with ID {request.OrderId} not found"
                };
            }

            return new OrderResponse
            {
                Success = true,
                Order = MapToDto(order)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", request.OrderId);
            return new OrderResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public override async Task<OrdersResponse> GetUserOrders(
        GetUserOrdersRequest request,
        ServerCallContext context)
    {
        try
        {
            var orders = await _orderRepository
                .GetAll()
                .Include(o => o.Items)
                .Where(o => o.UserId == request.UserId)
                .ToListAsync(context.CancellationToken);

            var response = new OrdersResponse { Success = true };
            response.Orders.AddRange(orders.Select(MapToDto));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId}", request.UserId);
            return new OrdersResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        var dto = new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId ?? string.Empty,
            Status = order.Status ?? string.Empty,
            TotalAmount = (double)order.TotalAmount,
            CreatedAtTicks = order.CreatedAt.Ticks
        };

        if (order.Items != null)
        {
            dto.Items.AddRange(order.Items.Select(i => new BuildingBlocks.Grpc.OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = (double)i.UnitPrice
            }));
        }

        return dto;
    }
}
