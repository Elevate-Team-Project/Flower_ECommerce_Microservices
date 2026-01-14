using Catalog_Service.Features.OccasionsFeature.GetAllOccasions;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.ViewOccasionProducts
{
    public static class Endpoints
    {

        public static void MapGetAllOccasionsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/occasions/{occasionId:int}/products",
    async (int occasionId, IMediator mediator) =>
    {
        var result = await mediator.Send(
            new GetAllOccasionProductsQuery(occasionId));

        return result.IsSuccess
            ? Results.Ok(result)
            : Results.NotFound(result);
    });
        }
    }
}

