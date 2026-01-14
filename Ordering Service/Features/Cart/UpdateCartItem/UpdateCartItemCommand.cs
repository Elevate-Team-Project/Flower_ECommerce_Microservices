using Ordering_Service.Features.Shared;
using MediatR;

namespace Ordering_Service.Features.Cart.UpdateCartItem
{
    public record UpdateCartItemCommand(int CartId, int newquantity) : IRequest<RequestResponse<UpdateCartItemDto>>;
}
