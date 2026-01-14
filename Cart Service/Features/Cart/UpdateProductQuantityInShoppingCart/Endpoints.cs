using Cart_Service.Features.Shared;
using MediatR;

namespace Cart_Service.Features.Cart.UpdateProductQuantityInShoppingCart
{
    public static class Endpoints
    {
        public static void MapUpdateItemQuantityEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPatch("/api/v1/cart/items/{productId}", async (
                int productId,
                int quantity,
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";

                var result = await mediator.Send(
                    new UpdateCartItemQuantityCommand(userId, productId, quantity)
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
                        "Quantity updated successfully"
                    ),
                    statusCode: 200
                );
            })
            .WithSummary("Update cart item quantity")
            .WithDescription("Updates quantity — removes product when quantity becomes zero.");
        }
    }
}
