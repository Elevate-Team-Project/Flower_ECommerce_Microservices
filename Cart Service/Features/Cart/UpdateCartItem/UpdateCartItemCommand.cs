using Cart_Service.Features.Shared;
using MediatR;

namespace Cart_Service.Features.Cart.UpdateCartItem
{
    public record UpdateCartItemCommand( int CartId , int newquantity) : IRequest<RequestResponse<UpdateCartItemDto>>;

}
