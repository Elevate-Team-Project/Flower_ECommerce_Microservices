using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.ProductsFeature.GetProductDetails
{
    public static class Endpoints
    {

        public static void MapGetProductDetails(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products/{id:int}", async (
                int id,
                IMediator mediator) =>
            {
                try
                {
                    var result = await mediator.Send(new GetProductDetailsQuery(id));

                    if (!result.IsSuccess)
                        return Results.NotFound(
                            EndpointResponse<object>.NotFoundResponse(result.Message)
                        );

                    return Results.Ok(
                        EndpointResponse<object>.SuccessResponse(
                            result.Data!,
                            "Product details retrieved successfully"
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
            .WithName("GetProductDetails")
            .WithTags("Products");
        }
    }
}
