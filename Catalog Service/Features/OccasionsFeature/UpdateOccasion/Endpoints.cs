using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.UpdateOccasion
{
    public static class OccasionEndpoints
    {
        public static void MapOccasionEndpoints(this WebApplication app)
        {
            app.MapPut("/api/occasions/{id:int}", async (int id, UpdateOccasionDto dto, IMediator mediator) =>
            {
                var command = new UpdateOccasionCommand(id, dto);
                var result = await mediator.Send(command);
                if (result == 0)
                    return Results.NotFound($"Occasion with id {id} not found.");

                return Results.Ok(new { id = result });
            })
            .WithOpenApi()
            .WithName("UpdateOccasion")
            .WithTags("Occasions");
        }
    }
}
