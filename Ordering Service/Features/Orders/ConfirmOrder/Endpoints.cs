using MediatR;
using Ordering_Service.Features.Orders.ConfirmOrder.ViewModels;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders.ConfirmOrder
{
    public static class Endpoints
    {
        public static void MapConfirmOrderEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/orders/{orderId}/confirm", async (
                int orderId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new ConfirmOrderCommand(orderId));

                if (!result.IsSuccess)
                {
                    return Results.Json(
                        EndpointResponse<RequestResponse<ConfirmOrderViewModel>>.ErrorResponse(
                            result.Message,
                            400
                        ),
                        statusCode: 400
                    );
                }

                return Results.Json(
                    EndpointResponse<RequestResponse<ConfirmOrderViewModel>>.SuccessResponse(
                        result,
                        "Order confirmed"
                    ),
                    statusCode: 200
                );
            })
            .WithSummary("Confirm order placement")
            .WithDescription("Validates and confirms order, returns order view + tracking link.");
        }
    }
}
