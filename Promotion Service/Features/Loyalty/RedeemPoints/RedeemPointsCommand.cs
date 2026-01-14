using MediatR;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Loyalty.RedeemPoints
{
    /// <summary>
    /// US-H03: Redeem Points at Checkout
    /// </summary>
    public record RedeemPointsCommand(
        string UserId,
        int Points,
        int? OrderId = null,
        decimal? OrderAmount = null
    ) : IRequest<EndpointResponse<RedemptionResult>>;

    public record RedemptionResult(
        int TransactionId,
        int PointsRedeemed,
        decimal DiscountValue,
        int RemainingBalance
    );
}
