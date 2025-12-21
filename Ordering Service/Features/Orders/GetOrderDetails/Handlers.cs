using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.GetOrderDetails
{
    public class GetOrderDetailsHandler : IRequestHandler<GetOrderDetailsQuery, EndpointResponse<OrderDetailDto>>
    {
        private readonly IBaseRepository<Order> _orderRepository;

        public GetOrderDetailsHandler(IBaseRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<EndpointResponse<OrderDetailDto>> Handle(
            GetOrderDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetAll()
                .Include(o => o.Items)
                .Include(o => o.Shipments)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

            if (order == null)
            {
                return EndpointResponse<OrderDetailDto>.NotFoundResponse("Order not found");
            }

            var dto = new OrderDetailDto(
                order.Id,
                order.UserId,
                order.CouponCode,
                order.SubTotal,
                order.DiscountAmount,
                order.ShippingCost,
                order.TotalAmount,
                order.Status,
                order.ShippingAddress,
                order.BillingAddress,
                order.PaymentMethod,
                order.PaymentTransactionId,
                order.PaidAt,
                order.ShippedAt,
                order.DeliveredAt,
                order.Notes,
                order.CreatedAt,
                order.Items.Select(i => new OrderItemDetailDto(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.ProductImageUrl,
                    i.UnitPrice,
                    i.Quantity,
                    i.TotalPrice
                )).ToList(),
                order.Shipments.Select(s => new ShipmentDetailDto(
                    s.Id,
                    s.TrackingNumber,
                    s.Carrier,
                    s.Status,
                    s.EstimatedDeliveryDate,
                    s.ActualDeliveryDate,
                    s.CurrentLocation
                )).ToList()
            );

            return EndpointResponse<OrderDetailDto>.SuccessResponse(dto, "Order details retrieved successfully");
        }
    }
}
