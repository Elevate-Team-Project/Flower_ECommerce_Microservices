using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.OffersFeature.CreateOffer
{
    public static class Endpoints
    {
        public static void MapCreateOfferEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/offers", async (
                [FromBody] CreateOfferCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/offers/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateCatalogOffer")
            .WithTags("Offers")
            .WithSummary("Create a new offer (US-G01)")
            .WithDescription("Creates a promotional offer that can apply to a product, category, or occasion.")
            .Produces<CreateOfferCommand>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
