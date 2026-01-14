using MediatR;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.LoyaltyFeature.GetTiers
{
    /// <summary>
    /// US-H04: Loyalty Tier System
    /// </summary>
    public record GetTiersQuery() : IRequest<EndpointResponse<List<TierDto>>>;

    public record TierDto(
        int Id,
        string Name,
        string? NameAr,
        string? Description,
        int MinPoints,
        decimal PointsMultiplier,
        decimal? DiscountPercentage,
        bool FreeShipping,
        int? BonusPointsOnBirthday,
        string? IconUrl,
        string? BadgeColor,
        int SortOrder
    );
}
