using BuildingBlocks.Interfaces;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ordering_Service.Features.Cart.RemoveCartItem
{
    public class RemoveCartItemHandler : IRequestHandler<RemoveCartItemCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Entities.Cart> _cartRepository;
        private readonly IBaseRepository<CartItem> _cartItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveCartItemHandler> _logger;

        public RemoveCartItemHandler(
            IBaseRepository<Entities.Cart> cartRepository,
            IBaseRepository<CartItem> cartItemRepository,
            IUnitOfWork unitOfWork,
            ILogger<RemoveCartItemHandler> logger)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<bool>> Handle(
            RemoveCartItemCommand request,
            CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.Get(c => c.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
                return EndpointResponse<bool>.NotFoundResponse("Cart not found");

            var item = await _cartItemRepository
                .Get(i => i.CartId == cart.Id && i.ProductId == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                return EndpointResponse<bool>.NotFoundResponse("Product not in cart");

            _cartItemRepository.Delete(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Removed product {ProductId} from cart for user {UserId}",
                request.ProductId, request.UserId);

            return EndpointResponse<bool>.SuccessResponse(true, "Product removed from cart");
        }
    }
}
