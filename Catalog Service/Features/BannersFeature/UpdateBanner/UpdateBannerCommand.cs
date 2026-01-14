using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.BannersFeature.CreateBanner;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.BannersFeature.UpdateBanner
{
    public record UpdateBannerCommand(
        int BannerId,
        string Title,
        string? TitleAr,
        string? Subtitle,
        string? SubtitleAr,
        string DesktopImageUrl,
        string? MobileImageUrl,
        string? CtaText,
        string? CtaTextAr,
        string? CtaLink,
        BannerPosition Position,
        int SortOrder,
        DateTime ValidFrom,
        DateTime ValidUntil,
        bool IsActive
    ) : IRequest<EndpointResponse<BannerDto>>;
}
