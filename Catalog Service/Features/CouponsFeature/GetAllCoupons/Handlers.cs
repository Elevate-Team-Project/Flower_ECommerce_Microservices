using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.CouponsFeature.CreateCoupon;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.CouponsFeature.GetAllCoupons
{
    public class GetAllCouponsHandler : IRequestHandler<GetAllCouponsQuery, EndpointResponse<List<CouponDto>>>
    {
        private readonly IBaseRepository<Coupon> _couponRepository;

        public GetAllCouponsHandler(IBaseRepository<Coupon> couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<EndpointResponse<List<CouponDto>>> Handle(
            GetAllCouponsQuery request,
            CancellationToken cancellationToken)
        {
            var coupons = await _couponRepository.GetAll()
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CouponDto(
                    c.Id,
                    c.Code,
                    c.Name,
                    c.NameAr,
                    c.Description,
                    c.Type,
                    c.DiscountValue,
                    c.MaxDiscountAmount,
                    c.MinOrderAmount,
                    c.MaxTotalUsage,
                    c.MaxUsagePerUser,
                    c.CurrentUsageCount,
                    c.ValidFrom,
                    c.ValidUntil,
                    c.IsActive,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<CouponDto>>.SuccessResponse(coupons, "Coupons retrieved successfully");
        }
    }
}
