using System;
using System.Threading.Tasks;
using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Catalog_Service.Features.ProductsFeature.StockManagement
{
    public class CodOrderApprovedConsumer : IConsumer<CodOrderApprovedEvent>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CodOrderApprovedConsumer> _logger;

        public CodOrderApprovedConsumer(
            IBaseRepository<Product> productRepo,
            IUnitOfWork unitOfWork,
            ILogger<CodOrderApprovedConsumer> logger)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CodOrderApprovedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Processing stock deduction for approved COD Order {OrderId}", msg.OrderId);

            foreach (var item in msg.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0;

                    if (product.StockQuantity <= product.MinStock)
                    {
                        _logger.LogWarning("Product {ProductId} is Low Stock! Current: {Current}, Min: {Min}", product.Id, product.StockQuantity, product.MinStock);

                        await context.Publish(new ProductLowStockEvent
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            CurrentStock = product.StockQuantity,
                            MinStock = product.MinStock
                        });
                    }

                    _logger.LogInformation("Stock updated for Product {ProductId}. New Stock: {StockQuantity}", product.Id, product.StockQuantity);
                }
                else
                {
                    _logger.LogWarning("Product {ProductId} from COD Order {OrderId} not found during stock deduction", item.ProductId, msg.OrderId);
                }
            }

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Stock deduction completed for COD Order {OrderId}", msg.OrderId);
        }
    }
}
