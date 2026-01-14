using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.CouponsFeature.ValidateCoupon
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
