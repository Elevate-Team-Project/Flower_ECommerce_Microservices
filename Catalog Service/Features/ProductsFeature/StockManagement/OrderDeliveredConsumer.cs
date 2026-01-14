using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature.StockManagement
{
    public class OrderDeliveredConsumer : IConsumer<OrderDeliveredEvent>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderDeliveredConsumer> _logger;

        public OrderDeliveredConsumer(
            IBaseRepository<Product> productRepo,
            IUnitOfWork unitOfWork,
            ILogger<OrderDeliveredConsumer> logger)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
        {
            _logger.LogInformation("Processing stock deduction for Order {OrderId}", context.Message.OrderId);

            foreach (var item in context.Message.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0; // Prevent negative stock logic (though logically it shouldn't happen if reserved correctly)

                    // Auto-deactivate if out of stock?
                    // if (product.StockQuantity <= 0) product.IsAvailable = false; 
                    // Better to keep IsAvailable true but let validation handle it? 
                    // Requirement says: US-A12 Activate/Deactivate. Usually out of stock != Deactivated (Hidden).
                    // But if Stock == 0, IsAvailable could be false to hide from listings if desired. 
                    // For now, let's just log low stock.

                    if (product.StockQuantity <= product.MinStock)
                    {
                        _logger.LogWarning("Product {ProductId} is Low Stock! Current: {Current}, Min: {Min}", product.Id, product.StockQuantity, product.MinStock);
                        // TODO: Send Notification via NotificationService if implemented
                    }

                    _productRepo.Update(product);
                }
                else
                {
                    _logger.LogWarning("Product {ProductId} from Order {OrderId} not found during stock deduction", item.ProductId, context.Message.OrderId);
                }
            }

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Stock deduction completed for Order {OrderId}", context.Message.OrderId);
        }
    }
}
