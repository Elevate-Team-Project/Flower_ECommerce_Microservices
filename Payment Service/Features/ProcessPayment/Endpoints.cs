using MediatR;

namespace Payment_Service.Features.ProcessPayment
{
    public static class PaymentEndpoints
    {
        public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/payments")
                .WithTags("Payments")
                .RequireAuthorization();

            // Create payment intent (frontend gets client secret for card collection)
            group.MapPost("/create-intent", async (
                CreatePaymentIntentRequest request,
                IMediator mediator,
                HttpContext context) =>
            {
                var userId = context.User.FindFirst("sub")?.Value 
                    ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var command = new CreatePaymentIntentCommand(
                    request.OrderId,
                    userId,
                    request.Amount,
                    request.Currency ?? "EGP"
                );

                var result = await mediator.Send(command);

                if (!result.Success)
                    return Results.BadRequest(new { error = result.ErrorMessage });

                return Results.Ok(new
                {
                    paymentIntentId = result.PaymentIntentId,
                    clientSecret = result.ClientSecret
                });
            })
            .WithName("CreatePaymentIntent")
            .WithOpenApi();

            // Process/confirm payment
            group.MapPost("/process", async (
                ProcessPaymentRequest request,
                IMediator mediator,
                HttpContext context) =>
            {
                var userId = context.User.FindFirst("sub")?.Value
                    ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var command = new ProcessPaymentCommand(
                    request.OrderId,
                    userId,
                    request.Amount,
                    request.Currency ?? "EGP",
                    request.PaymentMethodId
                );

                var result = await mediator.Send(command);

                if (!result.Success)
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        error = result.ErrorMessage,
                        paymentId = result.PaymentId,
                        status = result.Status.ToString()
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    paymentId = result.PaymentId,
                    paymentIntentId = result.PaymentIntentId,
                    status = result.Status.ToString(),
                    cardLast4 = result.CardLast4,
                    cardBrand = result.CardBrand
                });
            })
            .WithName("ProcessPayment")
            .WithOpenApi();

            // Get payment status by order
            group.MapGet("/order/{orderId:int}", async (
                int orderId,
                IMediator mediator) =>
            {
                // This would need a GetPaymentByOrderHandler
                // For now, return placeholder
                return Results.Ok(new { message = "Endpoint ready - handler to be implemented" });
            })
            .WithName("GetPaymentByOrder")
            .WithOpenApi();
        }
    }

    // Request DTOs
    public record CreatePaymentIntentRequest(
        int OrderId,
        decimal Amount,
        string? Currency
    );

    public record ProcessPaymentRequest(
        int OrderId,
        decimal Amount,
        string? Currency,
        string PaymentMethodId
    );
}
