using Stripe;

namespace Payment_Service.Services
{
    /// <summary>
    /// Stripe implementation of the payment provider.
    /// Uses Stripe SDK to create and manage payments.
    /// </summary>
    public class StripePaymentProvider : IPaymentProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentProvider> _logger;

        public StripePaymentProvider(
            IConfiguration configuration, 
            ILogger<StripePaymentProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            int orderId,
            string userId,
            Dictionary<string, string>? metadata = null)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Stripe uses smallest currency unit (piasters for EGP)
                    Currency = currency.ToLower(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", orderId.ToString() },
                        { "user_id", userId }
                    },
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    }
                };

                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        options.Metadata[kvp.Key] = kvp.Value;
                    }
                }

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Created PaymentIntent {PaymentIntentId} for Order {OrderId}, Amount: {Amount} {Currency}",
                    paymentIntent.Id, orderId, amount, currency);

                return new PaymentIntentResult(
                    Success: true,
                    PaymentIntentId: paymentIntent.Id,
                    ClientSecret: paymentIntent.ClientSecret,
                    ErrorMessage: null
                );
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent for Order {OrderId}", orderId);
                return new PaymentIntentResult(
                    Success: false,
                    PaymentIntentId: null,
                    ClientSecret: null,
                    ErrorMessage: ex.Message
                );
            }
        }

        public async Task<PaymentResult> ConfirmPaymentAsync(
            string paymentIntentId,
            string paymentMethodId)
        {
            try
            {
                var service = new PaymentIntentService();
                
                var confirmOptions = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                };

                var paymentIntent = await service.ConfirmAsync(paymentIntentId, confirmOptions);

                if (paymentIntent.Status == "succeeded")
                {
                    var cardDetails = paymentIntent.PaymentMethod?.Card;
                    
                    _logger.LogInformation(
                        "Payment {PaymentIntentId} succeeded", paymentIntentId);

                    return new PaymentResult(
                        Success: true,
                        PaymentIntentId: paymentIntent.Id,
                        CardLast4: cardDetails?.Last4,
                        CardBrand: cardDetails?.Brand,
                        ErrorMessage: null,
                        ErrorCode: null
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Payment {PaymentIntentId} has status {Status}", 
                        paymentIntentId, paymentIntent.Status);

                    return new PaymentResult(
                        Success: false,
                        PaymentIntentId: paymentIntent.Id,
                        CardLast4: null,
                        CardBrand: null,
                        ErrorMessage: $"Payment status: {paymentIntent.Status}",
                        ErrorCode: null
                    );
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error confirming payment {PaymentIntentId}", paymentIntentId);
                
                return new PaymentResult(
                    Success: false,
                    PaymentIntentId: paymentIntentId,
                    CardLast4: null,
                    CardBrand: null,
                    ErrorMessage: ex.Message,
                    ErrorCode: ex.StripeError?.Code
                );
            }
        }

        public async Task<bool> CancelPaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.CancelAsync(paymentIntentId);
                
                _logger.LogInformation("Cancelled PaymentIntent {PaymentIntentId}", paymentIntentId);
                return paymentIntent.Status == "canceled";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error cancelling payment {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<RefundResult> RefundPaymentAsync(
            string paymentIntentId,
            decimal? amount = null)
        {
            try
            {
                var service = new RefundService();
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId
                };

                if (amount.HasValue)
                {
                    options.Amount = (long)(amount.Value * 100);
                }

                var refund = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Created refund {RefundId} for PaymentIntent {PaymentIntentId}",
                    refund.Id, paymentIntentId);

                return new RefundResult(
                    Success: true,
                    RefundId: refund.Id,
                    RefundedAmount: (decimal)refund.Amount / 100,
                    ErrorMessage: null
                );
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error refunding payment {PaymentIntentId}", paymentIntentId);
                
                return new RefundResult(
                    Success: false,
                    RefundId: null,
                    RefundedAmount: 0,
                    ErrorMessage: ex.Message
                );
            }
        }
    }
}
