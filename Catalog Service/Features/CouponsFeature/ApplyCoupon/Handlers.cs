using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.CouponsFeature.ApplyCoupon
{
    public class ApplyCouponHandler : IRequestHandler<ApplyCouponCommand, EndpointResponse<CouponApplicationResult>>
    {
        private readonly IBaseRepository<Coupon> _couponRepository;
        private readonly IBaseRepository<CouponUsage> _usageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApplyCouponHandler> _logger;

        public ApplyCouponHandler(
            IBaseRepository<Coupon> couponRepository,
            IBaseRepository<CouponUsage> usageRepository,
            IUnitOfWork unitOfWork,
            ILogger<ApplyCouponHandler> logger)
        {
            _couponRepository = couponRepository;
            _usageRepository = usageRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<CouponApplicationResult>> Handle(
            ApplyCouponCommand request,
            CancellationToken cancellationToken)
        {
            var coupon = await _couponRepository.GetAll()
                .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpperInvariant(), cancellationToken);

            if (coupon == null)
                return EndpointResponse<CouponApplicationResult>.NotFoundResponse("Coupon not found");

            // Create usage record
            var usage = new CouponUsage
            {
                CouponId = coupon.Id,
                UserId = request.UserId,
                OrderId = request.OrderId,
                DiscountApplied = request.DiscountApplied,
                IpAddress = request.IpAddress,
                UsedAt = DateTime.UtcNow
            };

            await _usageRepository.AddAsync(usage);

            // Increment coupon usage counter
            coupon.CurrentUsageCount++;
            _couponRepository.Update(coupon);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Applied coupon {CouponCode} to order {OrderId} for user {UserId}. Discount: {Discount}",
                coupon.Code, request.OrderId, request.UserId, request.DiscountApplied);

            var result = new CouponApplicationResult(
                usage.Id,
                coupon.Id,
                coupon.Code,
                usage.DiscountApplied,
                usage.UsedAt
            );

            return EndpointResponse<CouponApplicationResult>.SuccessResponse(
                result, "Coupon applied successfully");
        }
    }
}
