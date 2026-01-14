using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;
using System.Text.Json;

namespace Promotion_Service.Features.Coupons.ValidateCoupon
{
    public class ValidateCouponHandler : IRequestHandler<ValidateCouponCommand, EndpointResponse<CouponValidationResult>>
    {
        private readonly IBaseRepository<Coupon> _couponRepository;
        private readonly IBaseRepository<CouponUsage> _usageRepository;

        public ValidateCouponHandler(
            IBaseRepository<Coupon> couponRepository,
            IBaseRepository<CouponUsage> usageRepository)
        {
            _couponRepository = couponRepository;
            _usageRepository = usageRepository;
        }

        public async Task<EndpointResponse<CouponValidationResult>> Handle(
            ValidateCouponCommand request,
            CancellationToken cancellationToken)
        {
            // Find coupon by code
            var coupon = await _couponRepository.GetAll()
                .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpperInvariant(), cancellationToken);

            if (coupon == null)
                return InvalidResult("Coupon code not found");

            // Check if active
            if (!coupon.IsActive)
                return InvalidResult("Coupon is not active");

            // Check validity period
            var now = DateTime.UtcNow;
            if (now < coupon.ValidFrom)
                return InvalidResult("Coupon is not yet valid");

            if (now > coupon.ValidUntil)
                return InvalidResult("Coupon has expired");

            // Check total usage limit
            if (coupon.MaxTotalUsage.HasValue && coupon.CurrentUsageCount >= coupon.MaxTotalUsage.Value)
                return InvalidResult("Coupon usage limit reached");

            // Check per-user usage limit
            if (coupon.MaxUsagePerUser.HasValue)
            {
                var userUsageCount = await _usageRepository.GetAll()
                    .CountAsync(u => u.CouponId == coupon.Id && u.UserId == request.UserId, cancellationToken);

                if (userUsageCount >= coupon.MaxUsagePerUser.Value)
                    return InvalidResult("You have already used this coupon the maximum number of times");
            }

            // Check minimum order amount
            if (coupon.MinOrderAmount.HasValue && request.OrderAmount < coupon.MinOrderAmount.Value)
                return InvalidResult($"Minimum order amount of {coupon.MinOrderAmount.Value:C} required");

            // Check applicable products
            if (!string.IsNullOrEmpty(coupon.ApplicableProducts) && request.ProductIds?.Any() == true)
            {
                var applicableProducts = JsonSerializer.Deserialize<List<int>>(coupon.ApplicableProducts);
                if (applicableProducts?.Any() == true && !request.ProductIds.Any(p => applicableProducts.Contains(p)))
                    return InvalidResult("Coupon is not applicable to any products in your cart");
            }

            // Check applicable categories
            if (!string.IsNullOrEmpty(coupon.ApplicableCategories) && request.CategoryIds?.Any() == true)
            {
                var applicableCategories = JsonSerializer.Deserialize<List<int>>(coupon.ApplicableCategories);
                if (applicableCategories?.Any() == true && !request.CategoryIds.Any(c => applicableCategories.Contains(c)))
                    return InvalidResult("Coupon is not applicable to any categories in your cart");
            }

            // Calculate discount
            decimal calculatedDiscount = coupon.Type switch
            {
                CouponType.Percentage => request.OrderAmount * (coupon.DiscountValue / 100),
                CouponType.FixedAmount => coupon.DiscountValue,
                CouponType.FreeShipping => 0, // Shipping handled separately
                _ => 0
            };

            // Apply max discount cap
            if (coupon.MaxDiscountAmount.HasValue && calculatedDiscount > coupon.MaxDiscountAmount.Value)
                calculatedDiscount = coupon.MaxDiscountAmount.Value;

            var result = new CouponValidationResult(
                true,
                null,
                coupon.Id,
                coupon.Code,
                coupon.Type,
                coupon.DiscountValue,
                coupon.MaxDiscountAmount,
                calculatedDiscount
            );

            return EndpointResponse<CouponValidationResult>.SuccessResponse(result, "Coupon is valid");
        }

        private EndpointResponse<CouponValidationResult> InvalidResult(string message)
        {
            var result = new CouponValidationResult(false, message, null, null, null, null, null, null);
            return EndpointResponse<CouponValidationResult>.ErrorResponse(message, 400, new() { message }) 
                with { Data = result };
        }
    }
}
