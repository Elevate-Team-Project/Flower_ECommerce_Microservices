using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.CouponsFeature.CreateCoupon
{
    public record CreateCouponCommand(
        string Code,
        string Name,
        string? NameAr,
        string? Description,
        string? DescriptionAr,
        CouponType Type,
        decimal DiscountValue,
        decimal? MaxDiscountAmount,
        decimal? MinOrderAmount,
        string? ApplicableCustomerGroups,
        string? ApplicableCategories,
        string? ApplicableProducts,
        int? MaxTotalUsage,
        int? MaxUsagePerUser,
        DateTime ValidFrom,
        DateTime ValidUntil,
        string? AdminNotes = null
    ) : IRequest<EndpointResponse<CouponDto>>;

    public record CouponDto(
        int Id,
        string Code,
        string Name,
        string? NameAr,
        string? Description,
        CouponType Type,
        decimal DiscountValue,
        decimal? MaxDiscountAmount,
        decimal? MinOrderAmount,
        int? MaxTotalUsage,
        int? MaxUsagePerUser,
        int CurrentUsageCount,
        DateTime ValidFrom,
        DateTime ValidUntil,
        bool IsActive,
        DateTime CreatedAt
    );
}
