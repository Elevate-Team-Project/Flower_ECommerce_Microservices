using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.LoyaltyFeature.RedeemPoints
{
    public static class Endpoints
    {
        public static void MapRedeemPointsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/loyalty/redeem", async (
                [FromBody] RedeemPointsRequest request,
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";

                var command = new RedeemPointsCommand(
                    userId,
                    request.Points,
                    request.OrderId,
                    request.OrderAmount
                );

                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("RedeemPoints")
            .WithTags("Loyalty")
            .WithSummary("Redeem loyalty points (US-H03)")
            .WithDescription("Redeems points at checkout. Max 50% of order can be paid with points.")
            .Produces<RedeemPointsCommand>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }

    public record RedeemPointsRequest(
        int Points,
        int? OrderId = null,
        decimal? OrderAmount = null
    );
}
