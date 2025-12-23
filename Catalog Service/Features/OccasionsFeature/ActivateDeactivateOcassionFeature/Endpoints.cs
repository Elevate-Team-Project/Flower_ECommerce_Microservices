using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.Build.Execution;

namespace Catalog_Service.Features.OccasionsFeature.UpdateOccasion
{
    public static class ADOccasionEndpoints
    {
        public static void MapADOccasionEndpoints(this WebApplication app)
        {
            app.MapPut("/api/adoccasions/{id:int}", async (int id, ActivateDeactivateOcassionDto dto, IMediator mediator) =>
            {
                var command = new ActivateDeactivateOcassionComand(id, dto);
                var result = await mediator.Send(command);
                if (!result.IsSuccess)
                    return Results.BadRequest(
                        EndpointResponse<string>.ErrorResponse($"Occasion with id {id} not found."));

                return Results.Ok(EndpointResponse<int>.SuccessResponse(result.Data!, result.Message));
            })
            .WithOpenApi()
            .WithName("ADOccasion")
            .WithTags("Occasions");
        }
    }
}
