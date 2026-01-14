using MediatR;

namespace Promotion_Service.Features.Banners.DeleteBanner
{
    public static class Endpoints
    {
        public static void MapDeleteBannerEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/banners/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteBannerCommand(id));
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("DeleteBanner")
            .WithTags("Banners")
            .WithSummary("Delete a banner")
            .Produces<DeleteBannerCommand>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
