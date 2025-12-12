using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.GetAllCategories
{
    public static class Endpoints
    {
        public static void MapGetAllCategoriesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/categories", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllCategoriesQuery());

                if (!result.IsSuccess)
                    return EndpointResponse<RequestResponse<List<CategoryViewModel>>>.ErrorResponse(result.Message);

                return EndpointResponse<RequestResponse<List<CategoryViewModel>>>.SuccessResponse(result);
            })

            .WithSummary("Get all active categories")
            .WithDescription("Returns all active categories with their image and name.");
        }
    }
}
