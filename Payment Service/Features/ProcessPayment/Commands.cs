using MediatR;
using Payment_Service.Entities;

namespace Payment_Service.Features.ProcessPayment
{
    /// <summary>
    /// Command to initiate payment processing.
    /// </summary>
    public record ProcessPaymentCommand(
        int OrderId,
        string UserId,
        decimal Amount,
        string Currency,
        string PaymentMethodId // Stripe payment method ID from frontend
    ) : IRequest<PaymentResponse>;

    /// <summary>
    /// Command to create a payment intent (for frontend to collect card details).
    /// </summary>
    public record CreatePaymentIntentCommand(
        int OrderId,
        string UserId,
        decimal Amount,
        string Currency
    ) : IRequest<PaymentIntentResponse>;

    public record PaymentResponse(
        bool Success,
        int PaymentId,
        string? PaymentIntentId,
        PaymentStatus Status,
        string? CardLast4,
        string? CardBrand,
        string? ErrorMessage
    );

    public record PaymentIntentResponse(
        bool Success,
        string? PaymentIntentId,
        string? ClientSecret,
        string? ErrorMessage
    );
}
