using Cart_Service.Features.Cart.ViewShoppingCart.VM;
using Cart_Service.Features.Shared;
using MediatR;

namespace Cart_Service.Features.Cart.ViewShoppingCart
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
