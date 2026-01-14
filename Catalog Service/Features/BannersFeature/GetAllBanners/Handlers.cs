using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.BannersFeature.CreateBanner;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.BannersFeature.GetAllBanners
{
    public class GetAllBannersHandler : IRequestHandler<GetAllBannersQuery, EndpointResponse<List<BannerDto>>>
    {
        private readonly IBaseRepository<Banner> _bannerRepository;

        public GetAllBannersHandler(IBaseRepository<Banner> bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<EndpointResponse<List<BannerDto>>> Handle(
            GetAllBannersQuery request,
            CancellationToken cancellationToken)
        {
            var banners = await _bannerRepository.GetAll()
                .OrderBy(b => b.Position)
                .ThenBy(b => b.SortOrder)
                .Select(b => new BannerDto(
                    b.Id,
                    b.Title,
                    b.TitleAr,
                    b.Subtitle,
                    b.DesktopImageUrl,
                    b.MobileImageUrl,
                    b.CtaText,
                    b.CtaLink,
                    b.Position,
                    b.SortOrder,
                    b.ValidFrom,
                    b.ValidUntil,
                    b.IsActive,
                    b.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<BannerDto>>.SuccessResponse(
                banners, "Banners retrieved successfully");
        }
    }
}
