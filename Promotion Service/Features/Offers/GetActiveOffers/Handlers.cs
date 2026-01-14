using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.GetActiveOffers
{
    public class GetActiveOffersHandler : IRequestHandler<GetActiveOffersQuery, EndpointResponse<List<ActiveOfferDto>>>
    {
        private readonly IBaseRepository<Offer> _offerRepository;

        public GetActiveOffersHandler(IBaseRepository<Offer> offerRepository)
        {
            _offerRepository = offerRepository;
        }

        public async Task<EndpointResponse<List<ActiveOfferDto>>> Handle(
            GetActiveOffersQuery request,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var query = _offerRepository.GetAll()
                .Where(o => o.Status == OfferStatus.Active)
                .Where(o => o.StartDate <= now && o.EndDate >= now);

            // Filter by target
            if (request.ProductId.HasValue)
            {
                query = query.Where(o =>
                    o.TargetType == OfferTargetType.Product && o.ProductId == request.ProductId ||
                    o.TargetType == OfferTargetType.All);
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(o =>
                    o.TargetType == OfferTargetType.Category && o.CategoryId == request.CategoryId ||
                    o.TargetType == OfferTargetType.All);
            }

            if (request.OccasionId.HasValue)
            {
                query = query.Where(o =>
                    o.TargetType == OfferTargetType.Occasion && o.OccasionId == request.OccasionId ||
                    o.TargetType == OfferTargetType.All);
            }

            var offers = await query
                .OrderByDescending(o => o.Priority)
                .ThenByDescending(o => o.DiscountValue)
                .Select(o => new ActiveOfferDto(
                    o.Id,
                    o.Name,
                    o.NameAr,
                    o.Description,
                    o.Type,
                    o.DiscountValue,
                    o.MaxDiscountAmount,
                    o.TargetType,
                    o.ProductId,
                    o.CategoryId,
                    o.OccasionId,
                    o.EndDate
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<ActiveOfferDto>>.SuccessResponse(
                offers,
                "Active offers retrieved successfully");
        }
    }
}
