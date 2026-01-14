using BuildingBlocks.Interfaces;
using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Banners.CreateBanner
{
    public class CreateBannerHandler : IRequestHandler<CreateBannerCommand, EndpointResponse<BannerDto>>
    {
        private readonly IBaseRepository<Banner> _bannerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBannerHandler(
            IBaseRepository<Banner> bannerRepository,
            IUnitOfWork unitOfWork)
        {
            _bannerRepository = bannerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<BannerDto>> Handle(
            CreateBannerCommand request,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var isActive = now >= request.ValidFrom && now <= request.ValidUntil;

            var banner = new Banner
            {
                Title = request.Title,
                TitleAr = request.TitleAr,
                Subtitle = request.Subtitle,
                SubtitleAr = request.SubtitleAr,
                DesktopImageUrl = request.DesktopImageUrl,
                MobileImageUrl = request.MobileImageUrl,
                CtaText = request.CtaText,
                CtaTextAr = request.CtaTextAr,
                CtaLink = request.CtaLink,
                Position = request.Position,
                SortOrder = request.SortOrder,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                IsActive = isActive
            };

            await _bannerRepository.AddAsync(banner);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(banner);
            return EndpointResponse<BannerDto>.SuccessResponse(dto, "Banner created successfully", 201);
        }

        private static BannerDto MapToDto(Banner banner) => new(
            banner.Id,
            banner.Title,
            banner.TitleAr,
            banner.Subtitle,
            banner.DesktopImageUrl,
            banner.MobileImageUrl,
            banner.CtaText,
            banner.CtaLink,
            banner.Position,
            banner.SortOrder,
            banner.ValidFrom,
            banner.ValidUntil,
            banner.IsActive,
            banner.CreatedAt
        );
    }
}
