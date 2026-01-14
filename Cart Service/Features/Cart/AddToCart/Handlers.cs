using BuildingBlocks.Grpc;
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
        private readonly CatalogGrpc.CatalogGrpcClient _catalogClient;
        private readonly ILogger<AddToCartHandler> _logger;

        public AddToCartHandler(
            IBaseRepository<Entities.Cart> cartRepository,
            IBaseRepository<CartItem> cartItemRepository,
            IUnitOfWork unitOfWork,
            CatalogGrpc.CatalogGrpcClient catalogClient,
            ILogger<AddToCartHandler> logger)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _catalogClient = catalogClient;
            _logger = logger;
        }

        public async Task<EndpointResponse<CartItemDto>> Handle(
            AddToCartCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Quantity <= 0)
                return EndpointResponse<CartItemDto>.ErrorResponse("Quantity must be greater than 0");

            // Validate product via gRPC
            ProductResponse productResponse;
            try
            {
                productResponse = await _catalogClient.GetProductAsync(
                    new GetProductRequest { ProductId = request.ProductId },
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call Catalog gRPC service");
                return EndpointResponse<CartItemDto>.ErrorResponse(
                    "Failed to validate product. Please try again.", 503);
            }

            if (!productResponse.Success || productResponse.Product == null)
            {
                return EndpointResponse<CartItemDto>.NotFoundResponse(
                    productResponse.ErrorMessage ?? "Product not found");
            }

            var product = productResponse.Product;

            if (!product.IsAvailable)
                return EndpointResponse<CartItemDto>.ErrorResponse("Product is not available");

            if (product.StockQuantity < request.Quantity)
                return EndpointResponse<CartItemDto>.ErrorResponse(
                    $"Insufficient stock. Available: {product.StockQuantity}");

            // Get or create cart for user
            var cart = await _cartRepository.Get(c => c.UserId == request.UserId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                cart = new Entities.Cart { UserId = request.UserId };
                await _cartRepository.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Check if product already in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            // Calculate price (use discounted price if available)
            var unitPrice = product.HasDiscount 
                ? (decimal)product.DiscountedPrice 
                : (decimal)product.Price;

            if (existingItem != null)
            {
                // Validate total quantity against stock
                var newQuantity = existingItem.Quantity + request.Quantity;
                if (newQuantity > product.StockQuantity)
                    return EndpointResponse<CartItemDto>.ErrorResponse(
                        $"Cannot add more. Maximum available: {product.StockQuantity}");

                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = unitPrice; // Update to current price
                _cartItemRepository.Update(existingItem);
            }
            else
            {
                existingItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    ProductName = product.Name,
                    UnitPrice = unitPrice,
                    Quantity = request.Quantity,
                    PictureUrl = product.ImageUrl
                };
                await _cartItemRepository.AddAsync(existingItem);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added product {ProductId} ({ProductName}) to cart for user {UserId}",
                request.ProductId, product.Name, request.UserId);

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
