using MediatR;

namespace Catalog_Service.Features.ProductsFeature.Search
{
    public static class Endpoints
    {
        public static void MapSearchProductsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/products/search", async (
                DTOs dto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new SearchProductsQuery(dto));

                if (!result.IsSuccess)
                    return Shared.EndpointResponse<
                        Shared.RequestResponse<List<ViewModel>>>
                        .ErrorResponse(result.Message);

                return Shared.EndpointResponse<
                    Shared.RequestResponse<List<ViewModel>>>
                    .SuccessResponse(result);
            })
            .WithSummary("Search products")
            .WithDescription(
                "Search products by name, price range, category, or occasion");
        }
    }
}
