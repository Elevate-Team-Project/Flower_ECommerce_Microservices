using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.CouponsFeature.ValidateCoupon
{
    public static class Endpoints
    {
        public static void MapValidateCouponEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/coupons/validate", async (
                [FromBody] ValidateCouponRequest request,
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";

                var command = new ValidateCouponCommand(
                    request.Code,
                    userId,
                    request.OrderAmount,
                    request.ProductIds,
                    request.CategoryIds
                );

                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("ValidateCoupon")
            .WithTags("Coupons")
            .WithSummary("Validate a coupon code")
            .WithDescription("Validates coupon eligibility and calculates discount amount")
            .Produces<ValidateCouponCommand>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }

    public record ValidateCouponRequest(
        string Code,
        decimal OrderAmount,
        List<int>? ProductIds = null,
        List<int>? CategoryIds = null
    );
}
