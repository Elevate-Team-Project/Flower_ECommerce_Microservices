using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.GetAllOccasions
{
    public static class Endpoints
    {

        public static void MapGetAllOccasionsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/catalog/occasions", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllOccasionsQuery());
                return Results.Json(result);
            })
            .WithTags("Occasions")
            .WithName("GetAllOccasions")
            .Produces<EndpointResponse<List<OccasionViewModel>>>(200);
        }
    }
}

