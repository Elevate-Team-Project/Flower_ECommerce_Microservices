using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.BannersFeature.CreateBanner;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.BannersFeature.UpdateBanner
{
    public class UpdateBannerHandler : IRequestHandler<UpdateBannerCommand, EndpointResponse<BannerDto>>
    {
        private readonly IBaseRepository<Banner> _bannerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBannerHandler(
            IBaseRepository<Banner> bannerRepository,
            IUnitOfWork unitOfWork)
        {
            _bannerRepository = bannerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<BannerDto>> Handle(
            UpdateBannerCommand request,
            CancellationToken cancellationToken)
        {
            var banner = await _bannerRepository.GetAll()
                .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken);

            if (banner == null)
                return EndpointResponse<BannerDto>.NotFoundResponse("Banner not found");

            banner.Title = request.Title;
            banner.TitleAr = request.TitleAr;
            banner.Subtitle = request.Subtitle;
            banner.SubtitleAr = request.SubtitleAr;
            banner.DesktopImageUrl = request.DesktopImageUrl;
            banner.MobileImageUrl = request.MobileImageUrl;
            banner.CtaText = request.CtaText;
            banner.CtaTextAr = request.CtaTextAr;
            banner.CtaLink = request.CtaLink;
            banner.Position = request.Position;
            banner.SortOrder = request.SortOrder;
            banner.ValidFrom = request.ValidFrom;
            banner.ValidUntil = request.ValidUntil;
            banner.IsActive = request.IsActive;

            _bannerRepository.Update(banner);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new BannerDto(
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

            return EndpointResponse<BannerDto>.SuccessResponse(dto, "Banner updated successfully");
        }
    }
}
