using MediatR;

namespace Catalog_Service.Features.OffersFeature.GetAllOffers
{
    public static class Endpoints
    {
        public static void MapGetAllOffersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/offers", async (
                string? status,
                string? sortBy,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllOffersQuery(status, sortBy));
                return Results.Ok(result);
            })
            .WithName("GetAllOffers")
            .WithTags("Offers")
            .WithSummary("Get all offers (US-G02)")
            .WithDescription("Lists all offers sorted by status (Active > Scheduled > Expired)")
            .Produces<GetAllOffersQuery>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
