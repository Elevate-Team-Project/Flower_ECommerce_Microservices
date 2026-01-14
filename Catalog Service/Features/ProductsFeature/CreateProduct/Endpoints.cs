using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.ProductsFeature.CreateProduct
{
    public static class Endpoints
    {
        public static void MapCreateProductEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/products", async ([FromBody] CreateProductCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                if (!result.IsSuccess)
                    return Results.BadRequest(result);
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("CreateProduct");
        }
    }
}

