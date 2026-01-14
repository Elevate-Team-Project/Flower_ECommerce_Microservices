using MediatR;

namespace Catalog_Service.Features.BannersFeature.GetAllBanners
{
    public static class Endpoints
    {
        public static void MapGetAllBannersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/banners", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllBannersQuery());
                return Results.Ok(result);
            })
            .WithName("GetAllBanners")
            .WithTags("Banners")
            .WithSummary("Get all banners (admin)")
            .Produces<GetAllBannersQuery>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
