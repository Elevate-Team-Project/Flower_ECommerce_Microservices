using Cart_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace Cart_Service.Features.Cart.UpdateCartItem
{
    public static class Endpoints
    {
        public static void MapUpdateCartItemEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPut("/cart/items/{cartId:int}",
                async (int cartId, UpdateCartItemRequest request, IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(new UpdateCartItemCommand(cartId, request.NewQuantity), ct);

                    if (!result.IsSuccess)
                        return Results.BadRequest(EndpointResponse<UpdateCartItemDto>.ErrorResponse(result.Message));

                    return Results.Ok(EndpointResponse<UpdateCartItemDto>.SuccessResponse(result.Data, result.Message));
                })
                .WithName("UpdateCartItem")
                .WithTags("Cart")
                .Accepts<UpdateCartItemRequest>("application/json");
        }
    }

    public record UpdateCartItemRequest(int NewQuantity);
}
