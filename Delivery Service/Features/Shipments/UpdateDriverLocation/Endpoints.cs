using MediatR;

namespace Delivery_Service.Features.Shipments.UpdateDriverLocation
{
    public static class Endpoints
    {
        public static void MapUpdateDriverLocationEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/shipments/{id:int}/location", async (
                int id,
                UpdateDriverLocationRequest request,
                IMediator mediator) =>
            {
                var command = new UpdateDriverLocationCommand(
                    id,
                    request.Latitude,
                    request.Longitude,
                    request.DriverName,
                    request.DriverPhone);
                    
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .WithName("UpdateDriverLocation")
            .WithTags("Delivery Tracking")
            .Produces(200);
        }
    }

    public record UpdateDriverLocationRequest(
        double Latitude,
        double Longitude,
        string? DriverName = null,
        string? DriverPhone = null
    );
}
