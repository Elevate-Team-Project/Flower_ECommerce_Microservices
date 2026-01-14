using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Delivery.Shipments.UpdateShipmentStatus
{
    public static class Endpoints
    {
        public static void MapUpdateShipmentStatusEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/shipments/{shipmentId}/status", async (int shipmentId, [FromBody] UpdateStatusRequest request, IMediator mediator) =>
            {
                var command = new UpdateShipmentStatusCommand(shipmentId, request.NewStatus, request.CurrentLocation, request.Notes);
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("UpdateShipmentStatus")
            .WithTags("Shipments")
            .RequireAuthorization();
        }
    }

    public record UpdateStatusRequest(string NewStatus, string? CurrentLocation = null, string? Notes = null);
}
