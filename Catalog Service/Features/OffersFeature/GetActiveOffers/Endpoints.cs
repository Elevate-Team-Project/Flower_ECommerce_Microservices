using MediatR;

namespace Catalog_Service.Features.OffersFeature.GetActiveOffers
{
    public static class Endpoints
    {
        public static void MapGetActiveOffersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/offers/active", async (
                int? productId,
                int? categoryId,
                int? occasionId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetActiveOffersQuery(productId, categoryId, occasionId));
                return Results.Ok(result);
            })
            .WithName("GetActiveOffers")
            .WithTags("Offers")
            .WithSummary("Get active offers for customers (US-G05)")
            .WithDescription("Returns currently active offers. Filter by product, category, or occasion.")
            .Produces<GetActiveOffersQuery>(StatusCodes.Status200OK);
            // No auth required - public endpoint for customers
        }
    }
}
