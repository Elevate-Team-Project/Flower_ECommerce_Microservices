using MediatR;
using Ordering_Service.Features.Orders.ConfirmOrder.ViewModels;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ConfirmOrder
{
    public class ConfirmOrderCommand : IRequest<RequestResponse<ConfirmOrderViewModel>>
    {
        public int OrderId { get; }

        public ConfirmOrderCommand(int orderId)
        {
            OrderId = orderId;
        }
    }
}
