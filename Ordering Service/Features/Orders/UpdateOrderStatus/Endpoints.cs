using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Orders.UpdateOrderStatus
{
    public static class Endpoints
    {
        public static void MapUpdateOrderStatusEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/orders/{orderId}/status", async (
                int orderId,
                [FromBody] UpdateOrderStatusRequest request,
                IMediator mediator) =>
            {
                var command = new UpdateOrderStatusCommand(orderId, request.NewStatus, request.Notes);
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("UpdateOrderStatus")
            .WithTags("Orders")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminPolicy"); // Admin only
        }
    }

    public record UpdateOrderStatusRequest(string NewStatus, string? Notes = null);
}
