using MediatR;
using Promotion_Service.Entities;

namespace Promotion_Service.Features.Banners.GetActiveBanners
{
    public static class Endpoints
    {
        public static void MapGetActiveBannersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/banners/active", async (
                BannerPosition? position,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetActiveBannersQuery(position));
                return Results.Ok(result);
            })
            .WithName("GetActiveBanners")
            .WithTags("Banners")
            .WithSummary("Get active banners")
            .WithDescription("Returns currently active banners, optionally filtered by position")
            .Produces<GetActiveBannersQuery>(StatusCodes.Status200OK);
            // No auth required - public endpoint
        }
    }
}
