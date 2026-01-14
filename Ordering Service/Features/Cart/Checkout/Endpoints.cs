using MediatR;
using Ordering_Service.Features.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Cart.Checkout
{
    public static class CheckoutEndpoints
    {
        public static void MapCheckoutEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cart/checkout", async (
                [FromBody] CheckoutRequest request,
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "anonymous";

                var command = new CheckoutCommand(
                    userId,
                    request.DeliveryAddressId,
                    request.ShippingAddress,
                    request.PaymentMethod,
                    request.Notes,
                    request.CouponCode,
                    request.IsGift,
                    request.RecipientName,
                    request.RecipientPhone,
                    request.GiftMessage
                );

                var result = await mediator.Send(command);

                return result.IsSuccess
                    ? Results.Created($"/api/orders/{result.Data?.OrderId}", result)
                    : Results.BadRequest(result);
            })
            .WithName("Checkout")
            .WithTags("Cart")
            .WithSummary("Checkout and place order (US-D07)")
            .WithDescription("Converts cart to order. Supports gift orders and multiple payment methods.")
            .Produces<EndpointResponse<CheckoutResultDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }

    public record CheckoutRequest(
        int? DeliveryAddressId,
        string? ShippingAddress,
        string PaymentMethod,
        string? Notes = null,
        string? CouponCode = null,
        bool IsGift = false,
        string? RecipientName = null,
        string? RecipientPhone = null,
        string? GiftMessage = null
    );
}
