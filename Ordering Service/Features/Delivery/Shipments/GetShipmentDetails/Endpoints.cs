using MediatR;

namespace Ordering_Service.Features.Delivery.Shipments.GetShipmentDetails
{
    public static class Endpoints
    {
        public static void MapGetShipmentDetailsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/shipments/{shipmentId}", async (int shipmentId, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetShipmentDetailsQuery(shipmentId));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("GetShipmentDetails")
            .WithTags("Shipments")
            .RequireAuthorization();

            app.MapGet("/api/shipments/tracking/{trackingNumber}", async (string trackingNumber, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetShipmentByTrackingQuery(trackingNumber));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("GetShipmentByTracking")
            .WithTags("Shipments");

            app.MapGet("/api/shipments/order/{orderId}", async (int orderId, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetOrderShipmentsQuery(orderId));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("GetOrderShipments")
            .WithTags("Shipments")
            .RequireAuthorization();
        }
    }
}
