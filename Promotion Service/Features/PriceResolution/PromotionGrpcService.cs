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
        private readonly ILogger<PromotionGrpcService> _logger;

        public PromotionGrpcService(
            IBaseRepository<Offer> offerRepository,
            ILogger<PromotionGrpcService> logger)
        {
            _offerRepository = offerRepository;
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


    }
}
