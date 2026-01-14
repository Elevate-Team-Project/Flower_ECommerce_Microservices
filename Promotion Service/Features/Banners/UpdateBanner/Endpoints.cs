using MediatR;
using Microsoft.AspNetCore.Mvc;
using Promotion_Service.Entities;

namespace Promotion_Service.Features.Banners.UpdateBanner
{
    public static class Endpoints
    {
        public static void MapUpdateBannerEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/banners/{id}", async (
                int id,
                [FromBody] UpdateBannerRequest request,
                IMediator mediator) =>
            {
                var command = new UpdateBannerCommand(
                    id,
                    request.Title,
                    request.TitleAr,
                    request.Subtitle,
                    request.SubtitleAr,
                    request.DesktopImageUrl,
                    request.MobileImageUrl,
                    request.CtaText,
                    request.CtaTextAr,
                    request.CtaLink,
                    request.Position,
                    request.SortOrder,
                    request.ValidFrom,
                    request.ValidUntil,
                    request.IsActive
                );

                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("UpdateBanner")
            .WithTags("Banners")
            .WithSummary("Update a banner")
            .Produces<UpdateBannerCommand>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminPolicy");
        }
    }

    public record UpdateBannerRequest(
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
    );
}
