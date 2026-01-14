using MediatR;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Loyalty.GetTiers
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
