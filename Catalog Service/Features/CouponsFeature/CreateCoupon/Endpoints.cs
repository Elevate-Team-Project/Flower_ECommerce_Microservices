using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.CouponsFeature.CreateCoupon
{
    public static class Endpoints
    {
        public static void MapCreateCouponEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/coupons", async (
                [FromBody] CreateCouponCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/coupons/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateCoupon")
            .WithTags("Coupons")
            .WithSummary("Create a new coupon")
            .Produces<CreateCouponCommand>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
