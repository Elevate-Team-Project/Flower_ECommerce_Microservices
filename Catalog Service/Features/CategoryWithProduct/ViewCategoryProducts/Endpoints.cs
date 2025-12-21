using MediatR;

namespace Catalog_Service.Features.CategoryWithProduct.ViewCategoryProducts
{
    public static class Endpoints
    {
        public static void MapViewCategoryProductsEndpoints(
            this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/categories/{categoryId}/products",
                async (int categoryId, IMediator mediator) =>
                {
                    var result = await mediator.Send(
                        new ViewCategoryProductsQuery(categoryId));

                    return Results.Ok(result);
                })
            .WithSummary("View products by category")
            .WithDescription( "Returns products (image, price) for a specific category");
        }
    }
}
