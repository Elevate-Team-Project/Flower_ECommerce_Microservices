using Ordering_Service.Features.Cart.ViewShoppingCart.VM;
using Ordering_Service.Features.Shared;
using MediatR;

namespace Ordering_Service.Features.Cart.ViewShoppingCart
{
    public class ViewCartQuery : IRequest<RequestResponse<CartViewModel>>
    {
        public string UserId { get; }

        public ViewCartQuery(string userId)
        {
            UserId = userId;
        }
    }
}
