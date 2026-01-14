using Ordering_Service.Features.Shared;
using MediatR;

namespace Ordering_Service.Features.Cart.RemoveProductQuantityInShoppingCart
{
    public class DecreaseCartItemCommand : IRequest<RequestResponse<bool>>
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public DecreaseCartItemCommand(string userId, int productId, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
