using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.ProductsFeature.ProductExist
{
    public static class Endpoints
    {
        public static void MapProductExistsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/products/exists", async (
                ProductExistsDto dto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new ProductExistsQuery(dto));

                return Results.Json(
                    EndpointResponse<RequestResponse<bool>>
                        .SuccessResponse(result),
                    statusCode: 200
                );
            })
            .WithName("ProductExists")
            .WithSummary("Check if product exists")
            .WithDescription("Returns true if a product with the given name exists.");
        }
    }
}
