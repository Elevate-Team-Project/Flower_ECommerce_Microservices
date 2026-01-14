using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Banners.CreateBanner;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Banners.UpdateBanner
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
