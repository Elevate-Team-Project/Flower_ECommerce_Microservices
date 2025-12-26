using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Orders.ConfirmOrder.ViewModels;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ConfirmOrder
{
    public class Handler : IRequestHandler<ConfirmOrderCommand, RequestResponse<ConfirmOrderViewModel>>
    {
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IBaseRepository<Order> orderRepo, IUnitOfWork unitOfWork)
        {
            _orderRepo = orderRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<ConfirmOrderViewModel>> Handle(
            ConfirmOrderCommand request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepo
                .Get(o => o.Id == request.OrderId)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.ShippingAddress,
                    o.PaymentMethod,

                    o.SubTotal,
                    o.ShippingCost,
                    o.DiscountAmount,
                    o.TotalAmount,

                    Items = o.Items.Select(i => new
                    {
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        i.TotalPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
                return RequestResponse<ConfirmOrderViewModel>.Fail("Order not found");

            if (!order.Items.Any())
                return RequestResponse<ConfirmOrderViewModel>.Fail("Order must contain at least one item");

            
            var dbOrder = await _orderRepo.GetByIdAsync(request.OrderId);
            dbOrder.Status = "Processing";

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var vm = new ConfirmOrderViewModel
            {
                OrderId = order.Id,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,

                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,

                Status = "Processing",

                Items = order.Items.Select(i => new ItemViewModel
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList(),

                TrackUrl = $"/api/v1/orders/{order.Id}/track"
            };

            return RequestResponse<ConfirmOrderViewModel>
                .Success(vm, "Order confirmed successfully");
        }
    }
}
