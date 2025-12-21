using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.UpdateOrderStatus
{
    public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusHandler(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<bool>> Handle(
            UpdateOrderStatusCommand request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetAll()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                return EndpointResponse<bool>.NotFoundResponse("Order not found");
            }

            // Validate status transition
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(request.NewStatus))
            {
                return EndpointResponse<bool>.ErrorResponse($"Invalid status. Valid statuses: {string.Join(", ", validStatuses)}");
            }

            order.Status = request.NewStatus;

            // Update timestamps based on status
            switch (request.NewStatus)
            {
                case "Shipped":
                    order.ShippedAt = DateTime.UtcNow;
                    break;
                case "Delivered":
                    order.DeliveredAt = DateTime.UtcNow;
                    break;
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                order.Notes = request.Notes;
            }

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, $"Order status updated to {request.NewStatus}");
        }
    }
}
