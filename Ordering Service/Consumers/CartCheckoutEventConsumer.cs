using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;
using Ordering_Service.Features.Orders.CreateOrder;

namespace Ordering_Service.Consumers
{
    public class CartCheckoutEventConsumer : IConsumer<CartCheckoutEvent>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CartCheckoutEventConsumer> _logger;

        public CartCheckoutEventConsumer(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            ILogger<CartCheckoutEventConsumer> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CartCheckoutEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Processing cart checkout event for user: {UserId}", msg.UserId);

            // Verify products exist and get current prices from Catalog Service
            var productIds = msg.Items.Select(i => i.ProductId).Distinct().ToList();
            var httpClient = _httpClientFactory.CreateClient("CatalogService");

            var tasks = productIds.Select(async id =>
            {
                try
                {
                    var response = await httpClient.GetAsync($"api/products/{id}", context.CancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Catalog Service returned status {Status} for product {ProductId}", response.StatusCode, id);
                        return null;
                    }
                    var wrapper = await response.Content.ReadFromJsonAsync<EndpointResponse<CatalogProductDto>>(cancellationToken: context.CancellationToken);
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

            // Create OrderItems list
            var orderItems = msg.Items.Select(i =>
            {
                if (!products.TryGetValue(i.ProductId, out var catalogProduct))
                {
                    throw new Exception($"Product {i.ProductId} was not found in catalog");
                }

                return new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = catalogProduct.Name,
                    ProductImageUrl = catalogProduct.ImageUrl ?? i.ImageUrl,
                    UnitPrice = catalogProduct.Price, // Use official catalog price
                    Quantity = i.Quantity
                };
            }).ToList();

            var order = new Order
            {
                UserId = msg.UserId,
                CouponCode = msg.CouponCode,
                SubTotal = msg.SubTotal,
                DiscountAmount = msg.DiscountAmount,
                ShippingCost = msg.DeliveryFee,
                TotalAmount = msg.TotalAmount,
                Status = "Pending",
                ShippingAddress = msg.ShippingAddress ?? string.Empty,
                BillingAddress = msg.ShippingAddress ?? string.Empty, // Default to shipping
                PaymentMethod = msg.PaymentMethod,
                Notes = msg.Notes,
                DeliveryAddressId = msg.DeliveryAddressId,
                IsGift = msg.IsGift,
                RecipientName = msg.RecipientName,
                RecipientPhone = msg.RecipientPhone,
                GiftMessage = msg.GiftMessage,
                Items = orderItems
            };

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Successfully saved checkout Order: {OrderId} for user: {UserId}", order.Id, msg.UserId);

            // Publish OrderPlacedEvent to RabbitMQ
            var orderPlaced = new OrderPlacedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddressId = order.DeliveryAddressId ?? 0,
                IsGift = order.IsGift,
                RecipientName = order.RecipientName,
                RecipientPhone = order.RecipientPhone,
                GiftMessage = order.GiftMessage,
                Items = order.Items.Select(i => new OrderPlacedItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await context.Publish(orderPlaced, context.CancellationToken);
            _logger.LogInformation("Published OrderPlacedEvent to RabbitMQ for Order: {OrderId}", order.Id);
        }
    }
}
