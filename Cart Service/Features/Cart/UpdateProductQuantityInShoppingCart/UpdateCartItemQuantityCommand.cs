using Cart_Service.Features.Shared;
using MediatR;

namespace Cart_Service.Features.Cart.UpdateProductQuantityInShoppingCart
{
    public class UpdateCartItemQuantityCommand : IRequest<RequestResponse<bool>>
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public UpdateCartItemQuantityCommand(string userId, int productId, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
