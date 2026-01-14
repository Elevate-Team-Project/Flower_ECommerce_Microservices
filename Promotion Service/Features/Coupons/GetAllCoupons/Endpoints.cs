using MediatR;

namespace Promotion_Service.Features.Coupons.GetAllCoupons
{
    public static class Endpoints
    {
        public static void MapGetAllCouponsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coupons", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllCouponsQuery());
                return Results.Ok(result);
            })
            .WithName("GetAllCoupons")
            .WithTags("Coupons")
            .WithSummary("Get all coupons")
            .Produces<GetAllCouponsQuery>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
