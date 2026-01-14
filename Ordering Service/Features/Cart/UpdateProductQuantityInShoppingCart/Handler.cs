using BuildingBlocks.Interfaces;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ordering_Service.Features.Cart.UpdateProductQuantityInShoppingCart
{
    public class Handler : IRequestHandler<UpdateCartItemQuantityCommand, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Entities.Cart> _cartRepo;
        private readonly IBaseRepository<CartItem> _cartItemRepo;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IBaseRepository<Entities.Cart> cartRepo,
            IBaseRepository<CartItem> cartItemRepo,
            IUnitOfWork unitOfWork)
        {
            _cartRepo = cartRepo;
            _cartItemRepo = cartItemRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<bool>> Handle(
            UpdateCartItemQuantityCommand request,
            CancellationToken cancellationToken)
        {
            var cartId = await _cartRepo
                .Get(c => c.UserId == request.UserId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (cartId == 0)
                return RequestResponse<bool>.Fail("Cart not found");

            var item = await _cartItemRepo
                .Get(i => i.CartId == cartId && i.ProductId == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                return RequestResponse<bool>.Fail("Item not found");

            if (request.Quantity <= 0)
            {
                _cartItemRepo.Delete(item);
            }
            else
            {
                item.Quantity = request.Quantity;
                _cartItemRepo.Update(item);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return RequestResponse<bool>.Success(true, "Quantity updated");
        }
    }
}
