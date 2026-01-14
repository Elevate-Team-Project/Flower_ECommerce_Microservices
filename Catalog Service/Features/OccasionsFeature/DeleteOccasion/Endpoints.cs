using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace Catalog_Service.Features.OccasionsFeature.DeleteOccasion;

public static class Endpoints
{
    public static void MapDeleteOccasionEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/occasions/{id:int}",
            async (int id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new DeleteOccasionCommand(id), ct);

                if (!result.IsSuccess)
                    return Results.BadRequest(
                        EndpointResponse<bool>.ErrorResponse(result.Message));

                return Results.Ok(
                    EndpointResponse<bool>.SuccessResponse(result.Data, result.Message));
            })
            .WithName("DeleteOccasion")
            .WithTags("Occasions");
    }
}
