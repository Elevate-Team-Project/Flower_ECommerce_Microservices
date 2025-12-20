using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.Build.Execution;

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
                if (!result.IsSuccess)
                    return Results.NotFound(
                        EndpointResponse<string>.NotFoundResponse($"Occasion with id {id} not found."));

                return Results.Ok(EndpointResponse<int>.SuccessResponse(result.Data!, result.Message));
            })
            .WithOpenApi()
            .WithName("UpdateOccasion")
            .WithTags("Occasions");
        }
    }
}
