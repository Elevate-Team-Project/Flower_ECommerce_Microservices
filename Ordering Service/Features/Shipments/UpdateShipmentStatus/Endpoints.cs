using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Shipments.UpdateShipmentStatus
{
    public static class Endpoints
    {
        public static void MapUpdateShipmentStatusEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/shipments/{shipmentId}/status", async (
                int shipmentId,
                [FromBody] UpdateShipmentStatusRequest request,
                IMediator mediator) =>
            {
                var command = new UpdateShipmentStatusCommand(
                    shipmentId,
                    request.NewStatus,
                    request.CurrentLocation,
                    request.Notes
                );
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("UpdateShipmentStatus")
            .WithTags("Shipments")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminPolicy");
        }
    }

    public record UpdateShipmentStatusRequest(
        string NewStatus,
        string? CurrentLocation = null,
        string? Notes = null
    );
}
