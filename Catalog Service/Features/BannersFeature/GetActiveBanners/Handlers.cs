using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.BannersFeature.CreateBanner;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.BannersFeature.GetActiveBanners
{
    public class GetActiveBannersHandler : IRequestHandler<GetActiveBannersQuery, EndpointResponse<List<BannerDto>>>
    {
        private readonly IBaseRepository<Banner> _bannerRepository;

        public GetActiveBannersHandler(IBaseRepository<Banner> bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<EndpointResponse<List<BannerDto>>> Handle(
            GetActiveBannersQuery request,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var query = _bannerRepository.GetAll()
                .Where(b => b.IsActive)
                .Where(b => b.ValidFrom <= now && b.ValidUntil >= now);

            if (request.Position.HasValue)
            {
                query = query.Where(b => b.Position == request.Position.Value);
            }

            var banners = await query
                .OrderBy(b => b.SortOrder)
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
                banners, "Active banners retrieved successfully");
        }
    }
}
