using MediatR;
using Cart_Service.Features.Shared;

namespace Cart_Service.Features.Cart.RemoveCartItem
{
    /// <summary>
    /// US-D04: Remove product from Shopping Cart
    /// </summary>
    public record RemoveCartItemCommand(
        string UserId,
        int ProductId
    ) : IRequest<EndpointResponse<bool>>;
}
