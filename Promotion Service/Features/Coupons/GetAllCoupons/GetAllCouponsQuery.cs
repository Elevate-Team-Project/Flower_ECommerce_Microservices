using MediatR;
using Promotion_Service.Features.Coupons.CreateCoupon;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Coupons.GetAllCoupons
{
    public record GetAllCouponsQuery() : IRequest<EndpointResponse<List<CouponDto>>>;
}
