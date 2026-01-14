using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Offers.CreateOffer;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.GetAllOffers
{
    public class GetAllOffersHandler : IRequestHandler<GetAllOffersQuery, EndpointResponse<List<OfferDto>>>
    {
        private readonly IBaseRepository<Offer> _offerRepository;

        public GetAllOffersHandler(IBaseRepository<Offer> offerRepository)
        {
            _offerRepository = offerRepository;
        }

        public async Task<EndpointResponse<List<OfferDto>>> Handle(
            GetAllOffersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _offerRepository.GetAll();

            // Filter by status
            if (!string.IsNullOrEmpty(request.Status))
            {
                query = request.Status.ToLower() switch
                {
                    "active" => query.Where(o => o.Status == OfferStatus.Active),
                    "scheduled" => query.Where(o => o.Status == OfferStatus.Scheduled),
                    "expired" => query.Where(o => o.Status == OfferStatus.Expired),
                    "disabled" => query.Where(o => o.Status == OfferStatus.Disabled),
                    _ => query
                };
            }

            // Sort - default by status (Active > Scheduled > Expired)
            query = request.SortBy?.ToLower() switch
            {
                "name" => query.OrderBy(o => o.Name),
                "startdate" => query.OrderByDescending(o => o.StartDate),
                "enddate" => query.OrderByDescending(o => o.EndDate),
                _ => query.OrderBy(o => o.Status).ThenByDescending(o => o.CreatedAt)
            };

            var offers = await query
                .Select(o => new OfferDto(
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
                    o.StartDate,
                    o.EndDate,
                    o.Status,
                    o.Priority,
                    o.IsActive,
                    o.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<OfferDto>>.SuccessResponse(offers, "Offers retrieved successfully");
        }
    }
}
