using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.ProductsFeature.GetBestSellers
{
    public static class Endpoints
    {
        public static void MapGetBestSellersEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products/best-sellers", async (
                int? count,
                IMediator mediator) =>
            {
                try
                {
                    var query = new GetBestSellersQuery(count ?? 10);
                    var result = await mediator.Send(query);

                    if (!result.IsSuccess)
                        return Results.BadRequest(
                            EndpointResponse<object>.ErrorResponse(result.Message)
                        );

                    return Results.Ok(
                        EndpointResponse<object>.SuccessResponse(
                            result.Data!,
                            "Best sellers retrieved successfully"
                        )
                    );
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Server Error",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("GetBestSellers")
            .WithTags("Products")
            .WithDescription("Get best selling products. Deactivated products are excluded.");
        }
    }
}
