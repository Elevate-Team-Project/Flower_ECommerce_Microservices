using BuildingBlocks.Interfaces;
using BuildingBlocks.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment_Service.Entities;
using Payment_Service.Services;

namespace Payment_Service.Features.ProcessPayment
{
    /// <summary>
    /// Handler for creating a payment intent (Step 1: Frontend gets client secret).
    /// </summary>
    public class CreatePaymentIntentHandler : IRequestHandler<CreatePaymentIntentCommand, PaymentIntentResponse>
    {
        private readonly IPaymentProvider _paymentProvider;
        private readonly IBaseRepository<Payment> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreatePaymentIntentHandler> _logger;

        public CreatePaymentIntentHandler(
            IPaymentProvider paymentProvider,
            IBaseRepository<Payment> paymentRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreatePaymentIntentHandler> logger)
        {
            _paymentProvider = paymentProvider;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PaymentIntentResponse> Handle(
            CreatePaymentIntentCommand request,
            CancellationToken cancellationToken)
        {
            // Create payment intent with Stripe
            var result = await _paymentProvider.CreatePaymentIntentAsync(
                request.Amount,
                request.Currency,
                request.OrderId,
                request.UserId);

            if (!result.Success)
            {
                return new PaymentIntentResponse(
                    Success: false,
                    PaymentIntentId: null,
                    ClientSecret: null,
                    ErrorMessage: result.ErrorMessage
                );
            }

            // Save payment record
            var payment = new Payment
            {
                OrderId = request.OrderId,
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                StripePaymentIntentId = result.PaymentIntentId,
                StripeClientSecret = result.ClientSecret,
                Status = PaymentStatus.Pending,
                PaymentMethod = "CreditCard"
            };

            await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created payment intent for Order {OrderId}, PaymentId: {PaymentId}",
                request.OrderId, payment.Id);

            return new PaymentIntentResponse(
                Success: true,
                PaymentIntentId: result.PaymentIntentId,
                ClientSecret: result.ClientSecret,
                ErrorMessage: null
            );
        }
    }

    /// <summary>
    /// Handler for confirming payment (Step 2: After frontend submits card).
    /// </summary>
    public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponse>
    {
        private readonly IPaymentProvider _paymentProvider;
        private readonly IBaseRepository<Payment> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProcessPaymentHandler> _logger;

        public ProcessPaymentHandler(
            IPaymentProvider paymentProvider,
            IBaseRepository<Payment> paymentRepository,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            ILogger<ProcessPaymentHandler> logger)
        {
            _paymentProvider = paymentProvider;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<PaymentResponse> Handle(
            ProcessPaymentCommand request,
            CancellationToken cancellationToken)
        {
            // Find existing payment record
            var payment = await _paymentRepository.Get(
                p => p.OrderId == request.OrderId && p.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
            {
                // Create new payment if not exists (direct payment without intent)
                var intentResult = await _paymentProvider.CreatePaymentIntentAsync(
                    request.Amount,
                    request.Currency,
                    request.OrderId,
                    request.UserId);

                if (!intentResult.Success)
                {
                    return new PaymentResponse(
                        Success: false,
                        PaymentId: 0,
                        PaymentIntentId: null,
                        Status: PaymentStatus.Failed,
                        CardLast4: null,
                        CardBrand: null,
                        ErrorMessage: intentResult.ErrorMessage
                    );
                }

                payment = new Payment
                {
                    OrderId = request.OrderId,
                    UserId = request.UserId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    StripePaymentIntentId = intentResult.PaymentIntentId,
                    Status = PaymentStatus.Processing,
                    PaymentMethod = "CreditCard"
                };
                await _paymentRepository.AddAsync(payment);
            }
            else
            {
                payment.Status = PaymentStatus.Processing;
                _paymentRepository.Update(payment);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Confirm payment with Stripe
            var confirmResult = await _paymentProvider.ConfirmPaymentAsync(
                payment.StripePaymentIntentId!,
                request.PaymentMethodId);

            if (confirmResult.Success)
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.PaidAt = DateTime.UtcNow;
                payment.CardLast4 = confirmResult.CardLast4;
                payment.CardBrand = confirmResult.CardBrand;
                _paymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish success event
                await _publishEndpoint.Publish(new PaymentSucceededEvent(
                    payment.OrderId,
                    payment.UserId,
                    payment.Amount,
                    payment.StripePaymentIntentId!,
                    payment.PaymentMethod,
                    payment.PaidAt.Value
                ), cancellationToken);

                _logger.LogInformation(
                    "Payment succeeded for Order {OrderId}, PaymentId: {PaymentId}",
                    request.OrderId, payment.Id);

                return new PaymentResponse(
                    Success: true,
                    PaymentId: payment.Id,
                    PaymentIntentId: payment.StripePaymentIntentId,
                    Status: PaymentStatus.Succeeded,
                    CardLast4: payment.CardLast4,
                    CardBrand: payment.CardBrand,
                    ErrorMessage: null
                );
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailedAt = DateTime.UtcNow;
                payment.ErrorMessage = confirmResult.ErrorMessage;
                payment.ErrorCode = confirmResult.ErrorCode;
                _paymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Note: PaymentFailedEvent should include order items for stock restoration
                // This would need to be passed from the caller or fetched via gRPC
                // For now, publish without items - Ordering Service will handle stock
                _logger.LogWarning(
                    "Payment failed for Order {OrderId}: {Error}",
                    request.OrderId, confirmResult.ErrorMessage);

                return new PaymentResponse(
                    Success: false,
                    PaymentId: payment.Id,
                    PaymentIntentId: payment.StripePaymentIntentId,
                    Status: PaymentStatus.Failed,
                    CardLast4: null,
                    CardBrand: null,
                    ErrorMessage: confirmResult.ErrorMessage
                );
            }
        }
    }
}
