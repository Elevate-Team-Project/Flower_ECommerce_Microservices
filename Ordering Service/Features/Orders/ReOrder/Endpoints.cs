using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ReOrder
{
    public static class Endpoints
    {
        public static void MapReOrderEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/orders/reorder", async (
                HttpContext context,
                [FromBody] ReOrderRequest request,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var command = new ReOrderCommand(
                    UserId: userId,
                    OriginalOrderId: request.OriginalOrderId,
                    Items: request.Items,
                    DeliveryAddressId: request.DeliveryAddressId,
                    ShippingAddress: request.ShippingAddress,
                    Notes: request.Notes
                );

                var result = await mediator.Send(command);

                return result.IsSuccess
                    ? Results.Created($"/api/orders/{result.Data?.NewOrderId}", result)
                    : result.StatusCode switch
                    {
                        404 => Results.NotFound(result),
                        401 => Results.Unauthorized(),
                        _ => Results.BadRequest(result)
                    };
            })
            .WithName("ReOrder")
            .WithTags("Orders")
            .WithSummary("Reorder a delivered order")
            .WithDescription("Creates a new order based on a previously delivered order. " +
                             "Allows modifying quantities (set to 0 to remove) and changing delivery address.")
            .Produces<EndpointResponse<ReOrderResponseDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

            // Optional: Get order details for reordering (preview what can be reordered)
            app.MapGet("/api/orders/{orderId}/reorder-preview", async (
                HttpContext context,
                int orderId,
                IMediator mediator) =>
            {
                var userId = context.User.Identity?.Name ?? "test-user";

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                // Use GetOrderDetails to fetch the order for preview
                var query = new GetOrderDetails.GetOrderDetailsQuery(orderId, userId);
                var result = await mediator.Send(query);

                if (!result.IsSuccess)
                    return Results.NotFound(result);

                if (result.Data?.Status != "Delivered")
                    return Results.BadRequest(
                        EndpointResponse<object>.ErrorResponse(
                            "Only delivered orders can be reordered"));

                return Results.Ok(result);
            })
            .WithName("GetReorderPreview")
            .WithTags("Orders")
            .WithSummary("Get order details for reorder preview")
            .WithDescription("Returns the details of a delivered order that can be reordered.")
            .Produces<EndpointResponse<GetOrderDetails.OrderDetailDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }

    /// <summary>
    /// Request body for reorder endpoint (without UserId which comes from auth)
    /// </summary>
    public record ReOrderRequest(
        int OriginalOrderId,
        List<ReOrderItemDto> Items,
        int? DeliveryAddressId = null,
        string? ShippingAddress = null,
        string? Notes = null
    );
}
