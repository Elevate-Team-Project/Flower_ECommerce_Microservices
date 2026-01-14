using MediatR;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Shipments.GetDeliveryTracking
{
    public static class Endpoints
    {
        public static void MapGetDeliveryTrackingEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/shipments/{id:int}/tracking", async (
                int id,
                IMediator mediator) =>
            {
                var query = new GetDeliveryTrackingQuery(id);
                var result = await mediator.Send(query);
                
                if (!result.IsSuccess)
                    return Results.NotFound(result);
                    
                return Results.Ok(result);
            })
            .WithName("GetDeliveryTracking")
            .WithTags("Delivery Tracking")
            .Produces<EndpointResponse<DeliveryTrackingDto>>(200)
            .Produces(404);

            // Get tracking by order ID (for customers)
            app.MapGet("/api/orders/{orderId:int}/tracking", async (
                int orderId,
                IMediator mediator,
                BuildingBlocks.Interfaces.IBaseRepository<Entities.Shipment> shipmentRepo) =>
            {
                var shipment = await shipmentRepo.GetByIdAsync(orderId);
                if (shipment == null)
                    return Results.NotFound(new { Message = "No shipment found for this order" });
                
                var query = new GetDeliveryTrackingQuery(shipment.Id);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetDeliveryTrackingByOrder")
            .WithTags("Delivery Tracking")
            .Produces<EndpointResponse<DeliveryTrackingDto>>(200)
            .Produces(404);
        }
    }
}

