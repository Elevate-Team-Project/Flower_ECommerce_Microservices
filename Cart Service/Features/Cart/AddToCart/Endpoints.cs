using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cart_Service.Features.Cart.AddToCart
{
    public static class AddToCartEndpoints
    {
        public static void MapAddToCartEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cart/items", async (
                [FromBody] AddToCartRequest request,
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "anonymous";

                var command = new AddToCartCommand(userId, request.ProductId, request.Quantity);
                var result = await mediator.Send(command);

                return result.IsSuccess
                    ? Results.Created($"/api/cart", result)
                    : Results.BadRequest(result);
            })
            .WithName("AddToCart")
            .WithTags("Cart")
            .WithSummary("Add product to cart (US-D01)")
            .Produces<EndpointResponse<CartItemDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }

    public record AddToCartRequest(int ProductId, int Quantity = 1);
}
