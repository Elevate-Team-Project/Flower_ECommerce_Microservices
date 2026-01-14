using MediatR;

namespace Promotion_Service.Features.Loyalty.GetTiers
{
    public static class Endpoints
    {
        public static void MapLoyaltyTiersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/loyalty/tiers", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetTiersQuery());
                return Results.Ok(result);
            })
            .WithName("GetLoyaltyTiers")
            .WithTags("Loyalty")
            .WithSummary("Get loyalty tier information (US-H04)")
            .WithDescription("Returns all tier levels with their benefits")
            .Produces<GetTiersQuery>(StatusCodes.Status200OK);
            // No auth required - public info
        }
    }
}
