using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.LoyaltyFeature.GetTiers
{
    public class GetTiersHandler : IRequestHandler<GetTiersQuery, EndpointResponse<List<TierDto>>>
    {
        private readonly IBaseRepository<LoyaltyTier> _tierRepository;

        public GetTiersHandler(IBaseRepository<LoyaltyTier> tierRepository)
        {
            _tierRepository = tierRepository;
        }

        public async Task<EndpointResponse<List<TierDto>>> Handle(
            GetTiersQuery request,
            CancellationToken cancellationToken)
        {
            var tiers = await _tierRepository.GetAll()
                .OrderBy(t => t.SortOrder)
                .Select(t => new TierDto(
                    t.Id,
                    t.Name,
                    t.NameAr,
                    t.Description,
                    t.MinPoints,
                    t.PointsMultiplier,
                    t.DiscountPercentage,
                    t.FreeShipping,
                    t.BonusPointsOnBirthday,
                    t.IconUrl,
                    t.BadgeColor,
                    t.SortOrder
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<TierDto>>.SuccessResponse(
                tiers, "Tiers retrieved successfully");
        }
    }
}
