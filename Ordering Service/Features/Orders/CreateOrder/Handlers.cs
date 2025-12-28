using BuildingBlocks.Interfaces;
using BuildingBlocks.ServiceClients;
using MediatR;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, EndpointResponse<CreateOrderDto>>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogClient;

        public CreateOrderHandler(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogClient)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _catalogClient = catalogClient;
        }

        public async Task<EndpointResponse<CreateOrderDto>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            // Validate products exist and get current prices from Catalog Service
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var productsResponse = await _catalogClient.GetProductsByIdsAsync(productIds, cancellationToken);
            
            if (!productsResponse.IsSuccess)
            {
                return EndpointResponse<CreateOrderDto>.ErrorResponse(
                    $"Failed to validate products: {productsResponse.ErrorMessage}", 
                    productsResponse.StatusCode);
            }

            var products = productsResponse.Data!.ToDictionary(p => p.Id);
            
            // Validate all products exist
            var missingProducts = productIds.Where(id => !products.ContainsKey(id)).ToList();
            if (missingProducts.Any())
            {
                return EndpointResponse<CreateOrderDto>.ErrorResponse(
                    $"Products not found: {string.Join(", ", missingProducts)}", 
                    404);
            }

            // Check product availability
            foreach (var item in request.Items)
            {
                var product = products[item.ProductId];
                if (!product.IsAvailable || product.StockQuantity < item.Quantity)
                {
                    return EndpointResponse<CreateOrderDto>.ErrorResponse(
                        $"Product '{product.Name}' is not available in the requested quantity", 
                        400);
                }
            }

            // Calculate totals using current prices from Catalog Service
            var orderItems = request.Items.Select(i =>
            {
                var catalogProduct = products[i.ProductId];
                var currentPrice = catalogProduct.DiscountedPrice ?? catalogProduct.Price;
                return new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = catalogProduct.Name,
                    ProductImageUrl = catalogProduct.ImageUrl ?? i.ProductImageUrl,
                    UnitPrice = currentPrice,
                    Quantity = i.Quantity
                };
            }).ToList();

            var subTotal = orderItems.Sum(i => i.UnitPrice * i.Quantity);
            decimal discountAmount = 0; // TODO: Calculate from coupon service
            decimal shippingCost = 0; // TODO: Calculate shipping from Delivery Service

            var order = new Order
            {
                UserId = request.UserId,
                CouponCode = request.CouponCode,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingCost = shippingCost,
                TotalAmount = subTotal - discountAmount + shippingCost,
                Status = "Pending",
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                Items = orderItems
            };

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new CreateOrderDto(
                order.Id,
                order.UserId,
                order.SubTotal,
                order.DiscountAmount,
                order.ShippingCost,
                order.TotalAmount,
                order.Status,
                order.CreatedAt
            );

            return EndpointResponse<CreateOrderDto>.SuccessResponse(dto, "Order created successfully", 201);
        }
    }
}

