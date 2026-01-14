using MediatR;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ViewMyOrders
{
    public static class Endpoints
    {
        public static void MapViewMyOrdersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/orders/my", async (
                HttpContext context,
                string? status,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";


                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var result = await mediator.Send(
                    new ViewMyOrdersQuery(userId, status)
                );

                return Results.Ok(
                    EndpointResponse<object>.SuccessResponse(result)
                );
            })
            .WithSummary("View my orders")
            .WithDescription("Returns active or completed user orders.");
        }
    }
}
