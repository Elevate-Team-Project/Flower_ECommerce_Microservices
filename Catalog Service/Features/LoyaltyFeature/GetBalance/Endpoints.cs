using MediatR;

namespace Catalog_Service.Features.LoyaltyFeature.GetBalance
{
    public static class Endpoints
    {
        public static void MapLoyaltyBalanceEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/loyalty/balance", async (
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";
                var result = await mediator.Send(new GetLoyaltyBalanceQuery(userId));
                return Results.Ok(result);
            })
            .WithName("GetLoyaltyBalance")
            .WithTags("Loyalty")
            .WithSummary("Get loyalty points balance (US-H02)")
            .WithDescription("Returns current points balance, tier info, and progress to next tier")
            .Produces<GetLoyaltyBalanceQuery>(StatusCodes.Status200OK)
            .RequireAuthorization();
        }
    }
}
