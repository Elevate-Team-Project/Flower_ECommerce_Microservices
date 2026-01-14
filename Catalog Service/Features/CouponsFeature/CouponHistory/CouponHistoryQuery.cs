using MediatR;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.CouponsFeature.CouponHistory
{
    public record GetCouponHistoryQuery(string UserId) : IRequest<EndpointResponse<CouponHistoryResult>>;

    public record CouponHistoryResult(
        List<CouponUsageDto> Usages,
        decimal TotalSavings
    );

    public record CouponUsageDto(
        int UsageId,
        string CouponCode,
        string CouponName,
        int? OrderId,
        decimal DiscountApplied,
        DateTime UsedAt
    );
}
