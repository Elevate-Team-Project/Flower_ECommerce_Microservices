using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Coupons.CouponHistory
{
    public class GetCouponHistoryHandler : IRequestHandler<GetCouponHistoryQuery, EndpointResponse<CouponHistoryResult>>
    {
        private readonly IBaseRepository<CouponUsage> _usageRepository;

        public GetCouponHistoryHandler(IBaseRepository<CouponUsage> usageRepository)
        {
            _usageRepository = usageRepository;
        }

        public async Task<EndpointResponse<CouponHistoryResult>> Handle(
            GetCouponHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var usages = await _usageRepository.GetAll()
                .Include(u => u.Coupon)
                .Where(u => u.UserId == request.UserId)
                .OrderByDescending(u => u.UsedAt)
                .Select(u => new CouponUsageDto(
                    u.Id,
                    u.Coupon.Code,
                    u.Coupon.Name,
                    u.OrderId,
                    u.DiscountApplied,
                    u.UsedAt
                ))
                .ToListAsync(cancellationToken);

            var totalSavings = usages.Sum(u => u.DiscountApplied);

            var result = new CouponHistoryResult(usages, totalSavings);
            return EndpointResponse<CouponHistoryResult>.SuccessResponse(
                result, "Coupon history retrieved successfully");
        }
    }
}
