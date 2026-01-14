namespace Payment_Service.Services
{
    /// <summary>
    /// Abstraction for payment processing providers.
    /// Allows for different implementations (Stripe, PayPal, Mock, etc.)
    /// </summary>
    public interface IPaymentProvider
    {
        /// <summary>
        /// Creates a payment intent for the specified amount.
        /// </summary>
        Task<PaymentIntentResult> CreatePaymentIntentAsync(
            decimal amount, 
            string currency, 
            int orderId, 
            string userId,
            Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Confirms a payment with the card token.
        /// </summary>
        Task<PaymentResult> ConfirmPaymentAsync(
            string paymentIntentId, 
            string paymentMethodId);

        /// <summary>
        /// Cancels a pending payment.
        /// </summary>
        Task<bool> CancelPaymentAsync(string paymentIntentId);

        /// <summary>
        /// Refunds a completed payment.
        /// </summary>
        Task<RefundResult> RefundPaymentAsync(
            string paymentIntentId, 
            decimal? amount = null);
    }

    public record PaymentIntentResult(
        bool Success,
        string? PaymentIntentId,
        string? ClientSecret,
        string? ErrorMessage
    );

    public record PaymentResult(
        bool Success,
        string? PaymentIntentId,
        string? CardLast4,
        string? CardBrand,
        string? ErrorMessage,
        string? ErrorCode
    );

    public record RefundResult(
        bool Success,
        string? RefundId,
        decimal RefundedAmount,
        string? ErrorMessage
    );
}
