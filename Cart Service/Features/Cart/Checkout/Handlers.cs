using BuildingBlocks.Interfaces;
using Cart_Service.Entities;
using Cart_Service.Features.Shared;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart_Service.Features.Cart.Checkout
{
    public class CheckoutHandler : IRequestHandler<CheckoutCommand, EndpointResponse<CheckoutResultDto>>
    {
        private readonly IBaseRepository<Entities.Cart> _cartRepository;
        private readonly IBaseRepository<CartItem> _cartItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CheckoutHandler> _logger;

        private const decimal DELIVERY_FEE = 50m;

        public CheckoutHandler(
            IBaseRepository<Entities.Cart> cartRepository,
            IBaseRepository<CartItem> cartItemRepository,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            ILogger<CheckoutHandler> logger)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
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
            decimal discountAmount = 0; // TODO: Validate coupon via Promotion Service

            var totalAmount = subTotal + DELIVERY_FEE - discountAmount;

            // Create order event to be consumed by Ordering Service
            var orderEvent = new CartCheckoutEvent
            {
                UserId = request.UserId,
                Items = cart.Items.Select(i => new CartCheckoutItem
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
                GiftMessage = request.GiftMessage
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
                OrderId: 0, // Will be assigned by Ordering Service
                OrderNumber: orderNumber,
                SubTotal: subTotal,
                DeliveryFee: DELIVERY_FEE,
                DiscountAmount: discountAmount,
                TotalAmount: totalAmount,
                Status: "Pending",
                EstimatedDelivery: DateTime.UtcNow.AddDays(3)
            );

            return EndpointResponse<CheckoutResultDto>.SuccessResponse(
                result, "Order placed successfully", 201);
        }
    }

    // Event to publish to Ordering Service
    public class CartCheckoutEvent
    {
        public string UserId { get; set; } = string.Empty;
        public List<CartCheckoutItem> Items { get; set; } = new();
        public int? DeliveryAddressId { get; set; }
        public string? ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CouponCode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsGift { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? GiftMessage { get; set; }
    }

    public class CartCheckoutItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
