using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Orders.GetOrderDetails
{
    public static class Endpoints
    {
        public static void MapGetOrderDetailsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/orders/{orderId}", async (
                int orderId,
                [FromHeader(Name = "X-User-Id")] string userId,
                IMediator mediator) =>
            {
                var query = new GetOrderDetailsQuery(orderId, userId);
                var result = await mediator.Send(query);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("GetOrderDetails")
            .WithTags("Orders")
            .Produces<OrderDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        }
    }
}
