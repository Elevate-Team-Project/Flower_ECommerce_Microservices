using Cart_Service.Features.Cart.ViewShoppingCart.VM;
using Cart_Service.Features.Shared;
using MediatR;

namespace Cart_Service.Features.Cart.ViewShoppingCart
{
    public static class Endpoints
    {
        public static void MapViewCartEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/cart", async (
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var userId = httpContext.User.Identity?.Name ?? "test-user";

                var result = await mediator.Send(new ViewCartQuery(userId));

                if (!result.IsSuccess)
                {
                    return Results.Json(
                        EndpointResponse<CartViewModel>.ErrorResponse(
                            result.Message,
                            statusCode: 400
                        ),
                        statusCode: 400
                    );
                }

                return Results.Json(
                    EndpointResponse<CartViewModel>.SuccessResponse(
                        result.Data!,
                        result.Message
                    ),
                    statusCode: 200
                );
            })
            .WithSummary("View shopping cart")
            .WithDescription("Displays cart items, subtotal, delivery fee, and total.");
        }
    }
}
