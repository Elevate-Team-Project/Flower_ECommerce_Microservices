using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature.StockManagement
{
    /// <summary>
    /// Consumer that restores stock when an order is cancelled.
    /// US-I02: Stock restoration on cancellation.
    /// </summary>
    public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderCancelledConsumer> _logger;

        public OrderCancelledConsumer(
            IBaseRepository<Product> productRepo,
            IUnitOfWork unitOfWork,
            ILogger<OrderCancelledConsumer> logger)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "Processing stock restoration for cancelled Order {OrderId}. Reason: {Reason}",
                message.OrderId, message.CancellationReason);

            foreach (var item in message.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    // Restore the reserved stock
                    product.StockQuantity += item.Quantity;

                    // Don't exceed max stock
                    if (product.MaxStock > 0 && product.StockQuantity > product.MaxStock)
                    {
                        product.StockQuantity = product.MaxStock;
                    }

                    _logger.LogInformation(
                        "Restored stock for Product {ProductId}. New Stock: {Stock}",
                        product.Id, product.StockQuantity);
                }
                else
                {
                    _logger.LogWarning(
                        "Product {ProductId} not found during stock restoration for Order {OrderId}",
                        item.ProductId, message.OrderId);
                }
            }

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation(
                "Stock restoration completed for cancelled Order {OrderId}",
                message.OrderId);
        }
    }
}
