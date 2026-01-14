using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Delivery.Shipments.CreateShipment
{
    public static class Endpoints
    {
        public static void MapCreateShipmentEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/shipments", async ([FromBody] CreateShipmentCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/shipments/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateShipment")
            .WithTags("Shipments")
            .RequireAuthorization();
        }
    }
}
