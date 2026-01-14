using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Promotion_Service.Features.Coupons.ApplyCoupon
{
    public static class Endpoints
    {
        public static void MapApplyCouponEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/coupons/apply", async (
                [FromBody] ApplyCouponRequest request,
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                var command = new ApplyCouponCommand(
                    request.Code,
                    userId,
                    request.OrderId,
                    request.OrderAmount,
                    request.DiscountApplied,
                    ipAddress
                );

                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("ApplyCoupon")
            .WithTags("Coupons")
            .WithSummary("Apply a coupon to an order")
            .WithDescription("Records coupon usage and increments usage counter")
            .Produces<ApplyCouponCommand>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }

    public record ApplyCouponRequest(
        string Code,
        int OrderId,
        decimal OrderAmount,
        decimal DiscountApplied
    );
}
