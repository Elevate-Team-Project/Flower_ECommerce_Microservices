using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.BannersFeature.CreateBanner
{
    public static class Endpoints
    {
        public static void MapCreateBannerEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/banners", async (
                [FromBody] CreateBannerCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/banners/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateBanner")
            .WithTags("Banners")
            .WithSummary("Create a promotional banner")
            .Produces<CreateBannerCommand>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
