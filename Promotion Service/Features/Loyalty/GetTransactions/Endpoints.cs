using MediatR;

namespace Promotion_Service.Features.Loyalty.GetTransactions
{
    public static class Endpoints
    {
        public static void MapLoyaltyTransactionsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/loyalty/transactions", async (
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";
                var result = await mediator.Send(new GetTransactionsQuery(userId));
                return Results.Ok(result);
            })
            .WithName("GetLoyaltyTransactions")
            .WithTags("Loyalty")
            .WithSummary("Get loyalty points history")
            .WithDescription("Returns all earned and redeemed points transactions")
            .Produces<GetTransactionsQuery>(StatusCodes.Status200OK)
            .RequireAuthorization();
        }
    }
}
