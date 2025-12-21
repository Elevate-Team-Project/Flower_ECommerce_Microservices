using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Shipments.CreateShipment
{
    public static class Endpoints
    {
        public static void MapCreateShipmentEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/shipments", async (
                [FromBody] CreateShipmentCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/shipments/{result.Data?.ShipmentId}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateShipment")
            .WithTags("Shipments")
            .Produces<CreateShipmentDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminPolicy");
        }
    }
}
