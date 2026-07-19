using System;
using System.Threading.Tasks;
using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering_Service.Entities;

namespace Ordering_Service.Consumers
{
    public class CodOrderApprovedConsumer : IConsumer<CodOrderApprovedEvent>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CodOrderApprovedConsumer> _logger;

        public CodOrderApprovedConsumer(
            IBaseRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CodOrderApprovedConsumer> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CodOrderApprovedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Processing COD order approval for Order: {OrderId}", msg.OrderId);

            var order = await _orderRepository.GetByIdAsync(msg.OrderId);
            if (order == null)
            {
                _logger.LogError("Order {OrderId} not found for COD approval status update", msg.OrderId);
                return;
            }

            order.Status = "Processing";

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Updated Order {OrderId} status to 'Processing' (COD Approved)", msg.OrderId);
        }
    }
}
