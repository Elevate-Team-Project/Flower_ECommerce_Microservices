using BuildingBlocks.Interfaces;
using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Coupons.CreateCoupon
{
    public class CreateCouponHandler : IRequestHandler<CreateCouponCommand, EndpointResponse<CouponDto>>
    {
        private readonly IBaseRepository<Coupon> _couponRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCouponHandler> _logger;

        public CreateCouponHandler(
            IBaseRepository<Coupon> couponRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateCouponHandler> logger)
        {
            _couponRepository = couponRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<CouponDto>> Handle(
            CreateCouponCommand request,
            CancellationToken cancellationToken)
        {
            var coupon = new Coupon
            {
                Code = request.Code.ToUpperInvariant(),
                Name = request.Name,
                NameAr = request.NameAr,
                Description = request.Description,
                DescriptionAr = request.DescriptionAr,
                Type = request.Type,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                MinOrderAmount = request.MinOrderAmount,
                ApplicableCustomerGroups = request.ApplicableCustomerGroups,
                ApplicableCategories = request.ApplicableCategories,
                ApplicableProducts = request.ApplicableProducts,
                MaxTotalUsage = request.MaxTotalUsage,
                MaxUsagePerUser = request.MaxUsagePerUser,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                AdminNotes = request.AdminNotes,
                IsActive = true
            };

            await _couponRepository.AddAsync(coupon);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created coupon {CouponId}: {CouponCode}", coupon.Id, coupon.Code);

            var dto = MapToDto(coupon);
            return EndpointResponse<CouponDto>.SuccessResponse(dto, "Coupon created successfully", 201);
        }

        private static CouponDto MapToDto(Coupon coupon) => new(
            coupon.Id,
            coupon.Code,
            coupon.Name,
            coupon.NameAr,
            coupon.Description,
            coupon.Type,
            coupon.DiscountValue,
            coupon.MaxDiscountAmount,
            coupon.MinOrderAmount,
            coupon.MaxTotalUsage,
            coupon.MaxUsagePerUser,
            coupon.CurrentUsageCount,
            coupon.ValidFrom,
            coupon.ValidUntil,
            coupon.IsActive,
            coupon.CreatedAt
        );
    }
}
