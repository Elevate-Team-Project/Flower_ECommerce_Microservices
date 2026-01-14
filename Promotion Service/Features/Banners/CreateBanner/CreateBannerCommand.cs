using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Banners.CreateBanner
{
    public record CreateBannerCommand(
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
        DateTime ValidUntil
    ) : IRequest<EndpointResponse<BannerDto>>;

    public record BannerDto(
        int Id,
        string Title,
        string? TitleAr,
        string? Subtitle,
        string DesktopImageUrl,
        string? MobileImageUrl,
        string? CtaText,
        string? CtaLink,
        BannerPosition Position,
        int SortOrder,
        DateTime ValidFrom,
        DateTime ValidUntil,
        bool IsActive,
        DateTime CreatedAt
    );
}
