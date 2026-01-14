using MediatR;

namespace Catalog_Service.Features.CouponsFeature.CouponHistory
{
    public static class Endpoints
    {
        public static void MapCouponHistoryEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coupons/history", async (
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";
                var result = await mediator.Send(new GetCouponHistoryQuery(userId));
                return Results.Ok(result);
            })
            .WithName("GetCouponHistory")
            .WithTags("Coupons")
            .WithSummary("Get user's coupon usage history")
            .WithDescription("Returns all coupons used by the user with total savings")
            .Produces<GetCouponHistoryQuery>(StatusCodes.Status200OK)
            .RequireAuthorization();
        }
    }
}
