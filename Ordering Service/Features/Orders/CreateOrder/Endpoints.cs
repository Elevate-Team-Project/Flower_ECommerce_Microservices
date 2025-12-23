using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Orders.CreateOrder
{
    public static class Endpoints
    {
        public static void MapCreateOrderEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/orders", async (
                [FromBody] CreateOrderCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/orders/{result.Data?.OrderId}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateOrder")
            .WithTags("Orders")
            .Produces<CreateOrderDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }
}
