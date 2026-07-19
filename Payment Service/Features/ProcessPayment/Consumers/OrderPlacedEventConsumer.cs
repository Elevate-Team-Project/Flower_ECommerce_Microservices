using System;
using System.Text.Json;
using System.Threading.Tasks;
using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment_Service.Entities;

namespace Payment_Service.Features.ProcessPayment.Consumers
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IBaseRepository<Payment> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderPlacedEventConsumer> _logger;

        public OrderPlacedEventConsumer(
            IBaseRepository<Payment> paymentRepository,
            IUnitOfWork unitOfWork,
            ILogger<OrderPlacedEventConsumer> logger)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Processing OrderPlacedEvent for Order: {OrderId}, Method: {Method}", msg.OrderId, msg.PaymentMethod);

            // Serialize items for later stock/shipment handling
            var itemsJson = JsonSerializer.Serialize(msg.Items);

            var payment = new Payment
            {
                OrderId = msg.OrderId,
                UserId = msg.UserId,
                Amount = msg.TotalAmount,
                Currency = "EGP",
                Status = PaymentStatus.Pending,
                PaymentMethod = msg.PaymentMethod,
                DeliveryAddressId = msg.DeliveryAddressId,
                IsGift = msg.IsGift,
                RecipientName = msg.RecipientName,
                RecipientPhone = msg.RecipientPhone,
                GiftMessage = msg.GiftMessage,
                ItemsJson = itemsJson
            };

            await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Saved Payment record for Order: {OrderId} with status: Pending", msg.OrderId);

            if (msg.PaymentMethod.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Approving Cash on Delivery Order: {OrderId}", msg.OrderId);

                // Publish CodOrderApprovedEvent to RMQ
                var codApproved = new CodOrderApprovedEvent
                {
                    OrderId = msg.OrderId,
                    UserId = msg.UserId,
                    TotalAmount = msg.TotalAmount,
                    DeliveryAddressId = msg.DeliveryAddressId,
                    IsGift = msg.IsGift,
                    RecipientName = msg.RecipientName,
                    RecipientPhone = msg.RecipientPhone,
                    GiftMessage = msg.GiftMessage,
                    Items = msg.Items.Select(i => new CodOrderApprovedItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                await context.Publish(codApproved, context.CancellationToken);
                _logger.LogInformation("Published CodOrderApprovedEvent for Order: {OrderId}", msg.OrderId);
            }
        }
    }
}
