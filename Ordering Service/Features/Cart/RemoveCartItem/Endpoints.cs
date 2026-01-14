using Ordering_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Cart.RemoveCartItem
{
    public static class RemoveCartItemEndpoints
    {
        public static void MapRemoveCartItemEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/cart/items/{productId}", async (
                int productId,
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "anonymous";

                var result = await mediator.Send(new RemoveCartItemCommand(userId, productId));

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("RemoveCartItem")
            .WithTags("Cart")
            .WithSummary("Remove product from cart (US-D04)")
            .Produces<EndpointResponse<bool>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        }
    }
}
