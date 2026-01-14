using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace Catalog_Service.Features.CategoriesFeature.DeleteCategory;

public static class Endpoints
{
    public static void MapDeleteCategoryEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{id:int}",
            async (int id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new DeleteCategoryCommand(id), ct);

                if (!result.IsSuccess)
                    return Results.BadRequest(EndpointResponse<bool>.ErrorResponse(result.Message));

                return Results.Ok(EndpointResponse<bool>.SuccessResponse(result.Data, result.Message));
            })
            .WithName("DeleteCategory")
            .WithTags("Categories");
    }
}
