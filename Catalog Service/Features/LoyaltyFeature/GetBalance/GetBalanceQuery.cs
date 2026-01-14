using MediatR;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.LoyaltyFeature.GetBalance
{
    /// <summary>
    /// US-H02: View Loyalty Points Balance
    /// </summary>
    public record GetLoyaltyBalanceQuery(string UserId) : IRequest<EndpointResponse<LoyaltyBalanceDto>>;

    public record LoyaltyBalanceDto(
        int AccountId,
        string UserId,
        int CurrentPoints,
        int TotalEarnedPoints,
        int TotalRedeemedPoints,
        string TierName,
        int TierId,
        decimal PointsMultiplier,
        decimal? TierDiscountPercentage,
        bool HasFreeShipping,
        int PointsToNextTier
    );
}
