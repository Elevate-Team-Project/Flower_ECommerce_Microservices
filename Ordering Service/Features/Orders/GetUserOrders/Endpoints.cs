using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.GetUserOrders
{
    public static class Endpoints
    {
        public static void MapGetUserOrdersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/orders/user/{userId}", async (
                string userId,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? status,
                IMediator mediator) =>
            {
                var query = new GetUserOrdersQuery(userId, page > 0 ? page : 1, pageSize > 0 ? pageSize : 10, status);
                var result = await mediator.Send(query);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("GetUserOrders")
            .WithTags("Orders")
            .Produces<PaginatedResult<OrderSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        }
    }
}
