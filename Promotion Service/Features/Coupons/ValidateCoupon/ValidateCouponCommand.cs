using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Coupons.ValidateCoupon
{
    public record ValidateCouponCommand(
        string Code,
        string UserId,
        decimal OrderAmount,
        List<int>? ProductIds = null,
        List<int>? CategoryIds = null
    ) : IRequest<EndpointResponse<CouponValidationResult>>;

    public record CouponValidationResult(
        bool IsValid,
        string? ErrorMessage,
        int? CouponId,
        string? Code,
        CouponType? Type,
        decimal? DiscountValue,
        decimal? MaxDiscountAmount,
        decimal? CalculatedDiscount
    );
}
