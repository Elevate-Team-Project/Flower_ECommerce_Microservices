using BuildingBlocks.Interfaces;
using MediatR;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, EndpointResponse<CreateOrderDto>>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderHandler(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<CreateOrderDto>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            // Calculate totals
            var subTotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal discountAmount = 0; // TODO: Calculate from coupon service
            decimal shippingCost = 0; // TODO: Calculate shipping

            var order = new Order
            {
                UserId = request.UserId,
                CouponCode = request.CouponCode,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingCost = shippingCost,
                TotalAmount = subTotal - discountAmount + shippingCost,
                Status = "Pending",
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                Items = request.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductImageUrl = i.ProductImageUrl,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new CreateOrderDto(
                order.Id,
                order.UserId,
                order.SubTotal,
                order.DiscountAmount,
                order.ShippingCost,
                order.TotalAmount,
                order.Status,
                order.CreatedAt
            );

            return EndpointResponse<CreateOrderDto>.SuccessResponse(dto, "Order created successfully", 201);
        }
    }
}
