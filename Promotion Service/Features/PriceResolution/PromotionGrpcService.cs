using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;

namespace Promotion_Service.Features.PriceResolution
{
    public class PromotionGrpcService : PromotionGrpc.PromotionGrpcBase
    {
        private readonly IBaseRepository<Offer> _offerRepository;
        private readonly IBaseRepository<Coupon> _couponRepository;
        private readonly IBaseRepository<LoyaltyAccount> _loyaltyAccountRepository;
        private readonly IBaseRepository<LoyaltyTier> _loyaltyTierRepository;
        private readonly ILogger<PromotionGrpcService> _logger;

        public PromotionGrpcService(
            IBaseRepository<Offer> offerRepository,
            IBaseRepository<Coupon> couponRepository,
            IBaseRepository<LoyaltyAccount> loyaltyAccountRepository,
            IBaseRepository<LoyaltyTier> loyaltyTierRepository,
            ILogger<PromotionGrpcService> logger)
        {
            _offerRepository = offerRepository;
            _couponRepository = couponRepository;
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _loyaltyTierRepository = loyaltyTierRepository;
            _logger = logger;
        }

        public override async Task<GetAdjustedPricesResponse> GetAdjustedPrices(
            GetAdjustedPricesRequest request, 
            ServerCallContext context)
        {
            var response = new GetAdjustedPricesResponse { Success = true };

            try
            {
                var now = DateTime.UtcNow;
                var activeOffers = await _offerRepository.GetAll()
                    .Where(o => o.Status == OfferStatus.Active && o.StartDate <= now && o.EndDate >= now)
                    .ToListAsync();

                foreach (var item in request.Items)
                {
                    var resultItem = new AdjustedPriceItem
                    {
                        ProductId = item.ProductId,
                        OriginalPrice = item.OriginalPrice,
                        NewPrice = item.OriginalPrice, // Default to original
                        HasDiscount = false,
                        DiscountAmount = 0
                    };

                    // Find best offer logic (Prioritize Product Offer > Category Offer)
                    
                    // 1. Check Product Offer
                    var productOffer = activeOffers
                        .FirstOrDefault(o => o.ProductId == item.ProductId);

                    if (productOffer != null)
                    {
                        ApplyOffer(resultItem, productOffer);
                    }
                    else
                    {
                        // 2. Check Category Offer
                        var categoryOffer = activeOffers
                            .FirstOrDefault(o => o.CategoryId == item.CategoryId);

                        if (categoryOffer != null)
                        {
                            ApplyOffer(resultItem, categoryOffer);
                        }
                    }

                    response.Items.Add(resultItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating adjusted prices");
                response.Success = false;
            }

            return response;
        }

        private void ApplyOffer(AdjustedPriceItem item, Offer offer)
        {
            decimal discount = 0;
            if (offer.Type == OfferType.Percentage)
            {
                discount = (decimal)item.OriginalPrice * (offer.DiscountValue / 100m);
                // Check for max discount amount
                if (offer.MaxDiscountAmount.HasValue && discount > offer.MaxDiscountAmount.Value)
                {
                    discount = offer.MaxDiscountAmount.Value;
                }
            }
            else // Fixed Amount
            {
                discount = offer.DiscountValue;
            }

            // Ensure discount doesn't exceed price
            if (discount > (decimal)item.OriginalPrice)
                discount = (decimal)item.OriginalPrice;

            item.DiscountAmount = (double)discount;
            item.NewPrice = item.OriginalPrice - (double)discount;
            item.HasDiscount = true;
            item.OfferName = offer.Name;
        }


        public override async Task<ValidateCouponResponse> ValidateCoupon(ValidateCouponRequest request, ServerCallContext context)
        {
            try
            {
                var coupon = await _couponRepository.Get(c => c.Code == request.CouponCode)
                    .FirstOrDefaultAsync();

                if (coupon == null)
                {
                    return new ValidateCouponResponse { Success = false, Message = "Invalid coupon code" };
                }

                var now = DateTime.UtcNow;
                if (coupon.ValidFrom > now || coupon.ValidUntil < now)
                {
                    return new ValidateCouponResponse { Success = false, Message = "Coupon is expired or not yet active" };
                }

                if (coupon.MinOrderAmount.HasValue && (decimal)request.CartTotal < coupon.MinOrderAmount.Value)
                {
                    return new ValidateCouponResponse { Success = false, Message = $"Minimum order value of {coupon.MinOrderAmount} required" };
                }

                // Check usage limit
                if (coupon.MaxTotalUsage.HasValue && coupon.CurrentUsageCount >= coupon.MaxTotalUsage.Value)
                {
                    return new ValidateCouponResponse { Success = false, Message = "Coupon usage limit reached" };
                }

                decimal discount = 0;
                if (coupon.Type == CouponType.Percentage)
                {
                    discount = (decimal)request.CartTotal * (coupon.DiscountValue / 100m);
                    if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                    {
                        discount = coupon.MaxDiscountAmount.Value;
                    }
                }
                else
                {
                    discount = coupon.DiscountValue;
                }

                // Cap at cart total
                if (discount > (decimal)request.CartTotal) discount = (decimal)request.CartTotal;

                return new ValidateCouponResponse { Success = true, DiscountAmount = (double)discount, Message = "Coupon applied" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coupon");
                return new ValidateCouponResponse { Success = false, Message = "System error validating coupon" };
            }
        }

        public override async Task<RedeemPointsResponse> RedeemPoints(RedeemPointsRequest request, ServerCallContext context)
        {
            try
            {
                var account = await _loyaltyAccountRepository.Get(la => la.UserId == request.UserId)
                    .Include(la => la.Transactions)
                    .FirstOrDefaultAsync();

                if (account == null)
                {
                    return new RedeemPointsResponse { Success = false, Message = "Loyalty account not found" };
                }

                if (account.CurrentPoints < request.PointsToRedeem)
                {
                    return new RedeemPointsResponse { Success = false, Message = "Insufficient points balance" };
                }

                // Get Active Tier to find redemption rate
                // Assuming standard rate if no tier or using Tier logic
                // Fetch all tiers to find match (or assuming account has Tier populated if included)
                // For simplicity, let's look up the Tier based on TotalPointsEarned or use a default rate
                
                // HARDCODED DEFAULT for MVP if Tier not found: 100 Points = $1
                decimal redemptionRate = 0.01m; 

                // Try to find actual tier
                var tiers = await _loyaltyTierRepository.GetAll().OrderByDescending(t => t.MinPoints).ToListAsync();
                var currentTier = tiers.FirstOrDefault(t => t.MinPoints <= account.TotalEarnedPoints);
                
                // If Tier has specific redemption rate, use it. (Assuming Entity has it, otherwise default)
                // Checking LoyaltyTier entity... (It usually has Rewards info, maybe not rate. Using standard 0.01)

                decimal discountAmount = request.PointsToRedeem * redemptionRate;

                // Cap at Cart Total
                if (discountAmount > (decimal)request.CartTotal)
                {
                     // Option: Cap it or Error? Let's cap money but keep points same (means bad value) OR Error.
                     // Better: Return error if trying to redeem more than cart worth? 
                     // Or just Cap. Let's cap the value.
                     discountAmount = (decimal)request.CartTotal;
                }

                return new RedeemPointsResponse 
                { 
                    Success = true, 
                    DiscountAmount = (double)discountAmount, 
                    Message = "Points valid for redemption" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming points");
                return new RedeemPointsResponse { Success = false, Message = "System error checking points" };
            }
        }
    }
}
