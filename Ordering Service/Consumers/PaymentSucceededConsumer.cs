using System;
using System.Threading.Tasks;
using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering_Service.Entities;

namespace Ordering_Service.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentSucceededConsumer> _logger;

        public PaymentSucceededConsumer(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<PaymentSucceededConsumer> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Processing payment succeeded event for Order: {OrderId}", msg.OrderId);

            var order = await _orderRepository.GetByIdAsync(msg.OrderId);
            if (order == null)
            {
                _logger.LogError("Order {OrderId} not found for payment status update", msg.OrderId);
                return;
            }

            order.Status = "Paid";
            order.PaidAt = msg.PaidAt;
            order.PaymentTransactionId = msg.PaymentIntentId;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Updated Order {OrderId} status to 'Paid'", msg.OrderId);
        }
    }
}
