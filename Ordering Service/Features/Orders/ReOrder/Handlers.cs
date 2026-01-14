using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ReOrder
{
    /// <summary>
    /// Handler for reordering a delivered order.
    /// US-E03: ReOrder a delivered order
    /// </summary>
    public class ReOrderHandler : IRequestHandler<ReOrderCommand, EndpointResponse<ReOrderResponseDto>>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CatalogGrpc.CatalogGrpcClient _catalogClient;
        private readonly ILogger<ReOrderHandler> _logger;

        public ReOrderHandler(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            CatalogGrpc.CatalogGrpcClient catalogClient,
            ILogger<ReOrderHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _catalogClient = catalogClient;
            _logger = logger;
        }

        public async Task<EndpointResponse<ReOrderResponseDto>> Handle(
            ReOrderCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Retrieve the original order with items
            var originalOrder = await _orderRepository.GetAll()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OriginalOrderId, cancellationToken);

            if (originalOrder == null)
            {
                return EndpointResponse<ReOrderResponseDto>.NotFoundResponse(
                    $"Order with ID {request.OriginalOrderId} not found");
            }

            // 2. Validate ownership
            if (originalOrder.UserId != request.UserId)
            {
                return EndpointResponse<ReOrderResponseDto>.UnauthorizedResponse(
                    "You can only reorder your own orders");
            }

            // 3. Validate order status is "Delivered"
            if (originalOrder.Status != "Delivered")
            {
                return EndpointResponse<ReOrderResponseDto>.ErrorResponse(
                    $"Only delivered orders can be reordered. Current status: {originalOrder.Status}",
                    400);
            }

            // 4. Apply modifications - filter items with quantity > 0
            var modifiedItems = request.Items
                .Where(i => i.Quantity > 0)
                .ToList();

            if (!modifiedItems.Any())
            {
                return EndpointResponse<ReOrderResponseDto>.ErrorResponse(
                    "At least one item must be included in the reorder",
                    400);
            }

            // 5. Validate products still exist and get current prices via Catalog gRPC
            var productIds = modifiedItems.Select(i => i.ProductId).ToList();
            var grpcRequest = new GetProductsByIdsRequest();
            grpcRequest.ProductIds.AddRange(productIds);

            ProductsResponse productsResponse;
            try
            {
                productsResponse = await _catalogClient.GetProductsByIdsAsync(
                    grpcRequest,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call Catalog gRPC service during reorder");
                return EndpointResponse<ReOrderResponseDto>.ErrorResponse(
                    $"Failed to validate products: {ex.Message}",
                    503);
            }

            if (!productsResponse.Success)
            {
                return EndpointResponse<ReOrderResponseDto>.ErrorResponse(
                    $"Failed to validate products: {productsResponse.ErrorMessage}",
                    400);
            }

            var products = productsResponse.Products.ToDictionary(p => p.Id);

            // Validate all products exist
            var missingProducts = productIds.Where(id => !products.ContainsKey(id)).ToList();
            if (missingProducts.Any())
            {
                return EndpointResponse<ReOrderResponseDto>.ErrorResponse(
                    $"Products no longer available: {string.Join(", ", missingProducts)}",
                    404);
            }

            // Check product availability
            foreach (var item in modifiedItems)
            {
                var product = products[item.ProductId];
                if (!product.IsAvailable)
                {
                    return EndpointResponse<ReOrderResponseDto>.ErrorResponse(
                        $"Product '{product.Name}' is no longer available",
                        400);
                }
                if (product.StockQuantity < item.Quantity)
                {
                    return EndpointResponse<ReOrderResponseDto>.ErrorResponse(
                        $"Product '{product.Name}' only has {product.StockQuantity} items in stock",
                        400);
                }
            }

            // 6. Create order items with current prices
            var orderItems = modifiedItems.Select(i =>
            {
                var catalogProduct = products[i.ProductId];
                var currentPrice = catalogProduct.HasDiscount
                    ? (decimal)catalogProduct.DiscountedPrice
                    : (decimal)catalogProduct.Price;
                return new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = catalogProduct.Name,
                    ProductImageUrl = catalogProduct.ImageUrl,
                    UnitPrice = currentPrice,
                    Quantity = i.Quantity
                };
            }).ToList();

            // Calculate totals
            var subTotal = orderItems.Sum(i => i.UnitPrice * i.Quantity);
            decimal discountAmount = 0; // TODO: Apply coupon if provided
            decimal shippingCost = originalOrder.ShippingCost; // Use same shipping cost as original

            // Determine shipping address
            var shippingAddress = !string.IsNullOrEmpty(request.ShippingAddress)
                ? request.ShippingAddress
                : originalOrder.ShippingAddress;

            // 7. Create the new order
            var newOrder = new Order
            {
                UserId = request.UserId,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingCost = shippingCost,
                TotalAmount = subTotal - discountAmount + shippingCost,
                Status = "Pending",
                ShippingAddress = shippingAddress,
                BillingAddress = originalOrder.BillingAddress,
                PaymentMethod = originalOrder.PaymentMethod,
                Notes = request.Notes ?? $"Reorder of Order #{originalOrder.Id}",
                DeliveryAddressId = request.DeliveryAddressId ?? originalOrder.DeliveryAddressId,
                // Preserve gift settings from original order
                IsGift = originalOrder.IsGift,
                RecipientName = originalOrder.RecipientName,
                RecipientPhone = originalOrder.RecipientPhone,
                GiftMessage = originalOrder.GiftMessage,
                Items = orderItems
            };

            await _orderRepository.AddAsync(newOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} reordered Order #{OriginalOrderId}. New Order #{NewOrderId} created",
                request.UserId, request.OriginalOrderId, newOrder.Id);

            // 8. Build response
            var responseItems = orderItems.Select(i => new ReOrderItemResponseDto(
                i.ProductId,
                i.ProductName,
                i.ProductImageUrl,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice
            )).ToList();

            var response = new ReOrderResponseDto(
                newOrder.Id,
                request.OriginalOrderId,
                newOrder.SubTotal,
                newOrder.DiscountAmount,
                newOrder.ShippingCost,
                newOrder.TotalAmount,
                newOrder.Status,
                newOrder.CreatedAt,
                newOrder.ShippingAddress,
                newOrder.DeliveryAddressId,
                responseItems
            );

            return EndpointResponse<ReOrderResponseDto>.SuccessResponse(
                response,
                "Order reordered successfully",
                201);
        }
    }
}
