using MediatR;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Coupons.ApplyCoupon
{
    public record ApplyCouponCommand(
        string Code,
        string UserId,
        int OrderId,
        decimal OrderAmount,
        decimal DiscountApplied,
        string? IpAddress = null
    ) : IRequest<EndpointResponse<CouponApplicationResult>>;

    public record CouponApplicationResult(
        int CouponUsageId,
        int CouponId,
        string Code,
        decimal DiscountApplied,
        DateTime AppliedAt
    );
}
