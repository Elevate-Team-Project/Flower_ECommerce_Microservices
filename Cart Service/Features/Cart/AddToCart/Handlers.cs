using BuildingBlocks.Interfaces;
using Cart_Service.Entities;
using Cart_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart_Service.Features.Cart.AddToCart
{
    public class AddToCartHandler : IRequestHandler<AddToCartCommand, EndpointResponse<CartItemDto>>
    {
        private readonly IBaseRepository<Entities.Cart> _cartRepository;
        private readonly IBaseRepository<CartItem> _cartItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddToCartHandler> _logger;

        public AddToCartHandler(
            IBaseRepository<Entities.Cart> cartRepository,
            IBaseRepository<CartItem> cartItemRepository,
            IUnitOfWork unitOfWork,
            ILogger<AddToCartHandler> logger)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<CartItemDto>> Handle(
            AddToCartCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Quantity <= 0)
                return EndpointResponse<CartItemDto>.ErrorResponse("Quantity must be greater than 0");

            // Get or create cart for user
            var cart = await _cartRepository.Get(c => c.UserId == request.UserId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                cart = new Entities.Cart
                {
                    UserId = request.UserId
                };
                await _cartRepository.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Check if product already in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += request.Quantity;
                _cartItemRepository.Update(existingItem);
            }
            else
            {
                // TODO: Validate product via gRPC and get current price
                // For now, we'll use placeholder values that should be updated
                // when gRPC client is configured

                existingItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    ProductName = $"Product {request.ProductId}", // Will be replaced by gRPC call
                    UnitPrice = 0, // Will be replaced by gRPC call
                    Quantity = request.Quantity,
                    PictureUrl = null
                };

                await _cartItemRepository.AddAsync(existingItem);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added product {ProductId} to cart for user {UserId}",
                request.ProductId, request.UserId);

            var dto = new CartItemDto(
                existingItem.Id,
                existingItem.ProductId,
                existingItem.ProductName,
                existingItem.UnitPrice,
                existingItem.Quantity,
                existingItem.PictureUrl,
                existingItem.UnitPrice * existingItem.Quantity
            );

            return EndpointResponse<CartItemDto>.SuccessResponse(dto, "Product added to cart");
        }
    }
}
