using MediatR;
using Catalog_Service.Features.CouponsFeature.CreateCoupon;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.CouponsFeature.GetAllCoupons
{
    public record GetAllCouponsQuery() : IRequest<EndpointResponse<List<CouponDto>>>;
}
