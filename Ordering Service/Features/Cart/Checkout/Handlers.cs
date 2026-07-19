using BuildingBlocks.Interfaces;
using Ordering_Service.Entities;
using Ordering_Service.Features.Shared;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.IntegrationEvents;

namespace Ordering_Service.Features.Cart.Checkout
{
    public class CheckoutHandler : IRequestHandler<CheckoutCommand, EndpointResponse<CheckoutResultDto>>
    {
        private readonly IBaseRepository<Entities.Cart> _cartRepository;
        private readonly IBaseRepository<CartItem> _cartItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CheckoutHandler> _logger;

        private const decimal DELIVERY_FEE = 50m;

        public CheckoutHandler(
            IBaseRepository<Entities.Cart> cartRepository,
            IBaseRepository<CartItem> cartItemRepository,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            IHttpClientFactory httpClientFactory,
            ILogger<CheckoutHandler> logger)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<EndpointResponse<CheckoutResultDto>> Handle(
            CheckoutCommand request,
            CancellationToken cancellationToken)
        {
            // Get cart with items
            var cart = await _cartRepository.Get(c => c.UserId == request.UserId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null || !cart.Items.Any())
                return EndpointResponse<CheckoutResultDto>.ErrorResponse("Cart is empty");

            // Validate delivery address
            if (!request.DeliveryAddressId.HasValue && string.IsNullOrEmpty(request.ShippingAddress))
                return EndpointResponse<CheckoutResultDto>.ErrorResponse("Delivery address is required");

            // Validate payment method
            if (request.PaymentMethod != "CashOnDelivery" && request.PaymentMethod != "CreditCard")
                return EndpointResponse<CheckoutResultDto>.ErrorResponse("Invalid payment method");

            // Calculate totals
            var subTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal discountAmount = 0; 
            
            var client = _httpClientFactory.CreateClient("CatalogService");

            // 1. Validate Coupon
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                try
                {
                    var couponReq = new { Code = request.CouponCode, OrderAmount = subTotal };
                    var httpResponse = await client.PostAsJsonAsync("/api/coupons/validate", couponReq, cancellationToken);
                    
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var responseResult = await httpResponse.Content.ReadFromJsonAsync<EndpointResponse<CouponValidationResultDto>>(cancellationToken: cancellationToken);
                        if (responseResult != null && responseResult.IsSuccess && responseResult.Data != null)
                        {
                            discountAmount += responseResult.Data.CalculatedDiscount ?? 0;
                        }
                        else
                        {
                            return EndpointResponse<CheckoutResultDto>.ErrorResponse($"Invalid Coupon: {responseResult?.Message ?? "Validation failed"}");
                        }
                    }
                    else
                    {
                        var errorResult = await httpResponse.Content.ReadFromJsonAsync<EndpointResponse<object>>(cancellationToken: cancellationToken);
                        return EndpointResponse<CheckoutResultDto>.ErrorResponse($"Invalid Coupon: {errorResult?.Message ?? "Validation failed"}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to validate coupon via HTTP");
                    return EndpointResponse<CheckoutResultDto>.ErrorResponse("Unable to validate coupon currently. Please try again.");
                }
            }

            // 2. Validate Loyalty Points
            decimal loyaltyDiscount = 0;
            if (request.PointsToRedeem.HasValue && request.PointsToRedeem.Value > 0)
            {
                try
                {
                    var redeemReq = new { Points = request.PointsToRedeem.Value, OrderAmount = subTotal - discountAmount };
                    var httpResponse = await client.PostAsJsonAsync("/api/loyalty/redeem", redeemReq, cancellationToken);

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var responseResult = await httpResponse.Content.ReadFromJsonAsync<EndpointResponse<RedemptionResultDto>>(cancellationToken: cancellationToken);
                        if (responseResult != null && responseResult.IsSuccess && responseResult.Data != null)
                        {
                            loyaltyDiscount = responseResult.Data.DiscountValue;
                            discountAmount += loyaltyDiscount;
                        }
                        else
                        {
                            return EndpointResponse<CheckoutResultDto>.ErrorResponse($"Loyalty Redemption Failed: {responseResult?.Message ?? "Redemption failed"}");
                        }
                    }
                    else
                    {
                        var errorResult = await httpResponse.Content.ReadFromJsonAsync<EndpointResponse<object>>(cancellationToken: cancellationToken);
                        return EndpointResponse<CheckoutResultDto>.ErrorResponse($"Loyalty Redemption Failed: {errorResult?.Message ?? "Redemption failed"}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to redeem points via HTTP");
                    return EndpointResponse<CheckoutResultDto>.ErrorResponse("Unable to process points redemption. Please try again.");
                }
            }

            var totalAmount = subTotal + DELIVERY_FEE - discountAmount;

            // Create order event to be consumed by Ordering Service
            var orderEvent = new CartCheckoutEvent
            {
                UserId = request.UserId,
                Items = cart.Items.Select(i => new CartCheckoutItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    ImageUrl = i.PictureUrl
                }).ToList(),
                DeliveryAddressId = request.DeliveryAddressId,
                ShippingAddress = request.ShippingAddress,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                CouponCode = request.CouponCode,
                SubTotal = subTotal,
                DeliveryFee = DELIVERY_FEE,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                IsGift = request.IsGift,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                GiftMessage = request.GiftMessage,
                PointsRedeemed = request.PointsToRedeem ?? 0
            };

            // Publish checkout event for Ordering Service
            await _publishEndpoint.Publish(orderEvent, cancellationToken);

            // Clear cart after successful checkout
            foreach (var item in cart.Items.ToList())
            {
                _cartItemRepository.HardDelete(item);
            }
            cart.CouponCode = null;
            _cartRepository.Update(cart);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Checkout completed for user {UserId}. Total: {Total}",
                request.UserId, totalAmount);

            // Generate unique order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var result = new CheckoutResultDto(
                OrderId: 0, 
                OrderNumber: orderNumber,
                SubTotal: subTotal,
                DeliveryFee: DELIVERY_FEE,
                DiscountAmount: discountAmount,
                TotalAmount: totalAmount,
                Status: "Pending",
                EstimatedDelivery: DateTime.UtcNow.AddDays(3),
                IsGift: request.IsGift,
                RecipientName: request.RecipientName,
                GiftMessage: request.GiftMessage
            );

            return EndpointResponse<CheckoutResultDto>.SuccessResponse(
                result, "Order placed successfully", 201);
        }
    }

    public record CouponValidationResultDto(
        bool IsValid,
        string? ErrorMessage,
        int? CouponId,
        string? Code,
        int? Type,
        decimal? DiscountValue,
        decimal? MaxDiscountAmount,
        decimal? CalculatedDiscount
    );

    public record RedemptionResultDto(
        int TransactionId,
        int PointsRedeemed,
        decimal DiscountValue,
        int RemainingBalance
    );
}
