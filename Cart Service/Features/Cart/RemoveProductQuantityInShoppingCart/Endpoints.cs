using Cart_Service.Features.Shared;
using MediatR;

namespace Cart_Service.Features.Cart.RemoveProductQuantityInShoppingCart
{
    public static class Endpoints
    {
        public static void MapDecreaseItemEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPatch("/api/v1/cart/items/{productId}/decrease", async (
    int productId,
    int quantity,
    HttpContext context,
    IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";

                var result = await mediator.Send(
                    new DecreaseCartItemCommand(userId, productId, quantity)
                );

                if (!result.IsSuccess)
                {
                    return Results.Json(
                        EndpointResponse<RequestResponse<bool>>.ErrorResponse(
                            result.Message,
                            400
                        ),
                        statusCode: 400
                    );
                }

                return Results.Json(
                    EndpointResponse<RequestResponse<bool>>.SuccessResponse(
                        result,
                        "Quantity updated successfully",
                        200
                    ),
                    statusCode: 200
                );
            })

            .WithSummary("Decrease product quantity in cart")
            .WithDescription("Decreases quantity — removes product if quantity becomes zero.");
        }
    }
}
