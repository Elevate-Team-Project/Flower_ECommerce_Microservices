using BuildingBlocks.Interfaces;
using MediatR;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace Ordering_Service.Features.Orders.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, EndpointResponse<CreateOrderDto>>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateOrderHandler> _logger;

        public CreateOrderHandler(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            ILogger<CreateOrderHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<EndpointResponse<CreateOrderDto>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            // Validate products exist and get current prices from Catalog Service via HTTP
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            
            var httpClient = _httpClientFactory.CreateClient("CatalogService");
            var tasks = productIds.Select(async id =>
            {
                try
                {
                    var response = await httpClient.GetAsync($"api/products/{id}", cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Catalog Service returned status {Status} for product {ProductId}", response.StatusCode, id);
                        return null;
                    }
                    var wrapper = await response.Content.ReadFromJsonAsync<EndpointResponse<CatalogProductDto>>(cancellationToken: cancellationToken);
                    return wrapper?.Data;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to call Catalog HTTP service for product {ProductId}", id);
                    return null;
                }
            });

            var productsList = (await Task.WhenAll(tasks)).Where(p => p != null).Select(p => p!).ToList();
            var products = productsList.ToDictionary(p => p.Id);
            
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
                var currentPrice = catalogProduct.HasDiscount && catalogProduct.DiscountedPrice.HasValue
                    ? catalogProduct.DiscountedPrice.Value 
                    : catalogProduct.Price;
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
