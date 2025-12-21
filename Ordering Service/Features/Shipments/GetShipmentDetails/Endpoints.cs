using MediatR;

namespace Ordering_Service.Features.Shipments.GetShipmentDetails
{
    public static class Endpoints
    {
        public static void MapGetShipmentDetailsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/shipments/{shipmentId}", async (
                int shipmentId,
                IMediator mediator) =>
            {
                var query = new GetShipmentDetailsQuery(shipmentId);
                var result = await mediator.Send(query);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("GetShipmentDetails")
            .WithTags("Shipments")
            .Produces<ShipmentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

            // Also add endpoint to get shipment by tracking number
            app.MapGet("/api/shipments/tracking/{trackingNumber}", async (
                string trackingNumber,
                IMediator mediator) =>
            {
                var query = new GetShipmentByTrackingQuery(trackingNumber);
                var result = await mediator.Send(query);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("GetShipmentByTracking")
            .WithTags("Shipments")
            .Produces<ShipmentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }

    public record GetShipmentByTrackingQuery(string TrackingNumber) : MediatR.IRequest<Shared.EndpointResponse<ShipmentDto>>;
}
