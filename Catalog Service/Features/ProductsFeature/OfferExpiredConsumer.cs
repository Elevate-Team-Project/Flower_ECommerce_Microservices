using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature
{
    /// <summary>
    /// Consumer that resets product discounted prices when an offer expires.
    /// US-G01: Expired offers automatically deactivate and reset prices.
    /// </summary>
    public class OfferExpiredConsumer : IConsumer<OfferExpiredEvent>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OfferExpiredConsumer> _logger;

        public OfferExpiredConsumer(
            IBaseRepository<Product> productRepo,
            IUnitOfWork unitOfWork,
            ILogger<OfferExpiredConsumer> logger)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OfferExpiredEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "Processing expired offer {OfferId}: {OfferName}",
                message.OfferId, message.OfferName);

            IQueryable<Product> productsQuery = _productRepo.GetAll();

            // Filter products based on offer target type
            switch (message.TargetType)
            {
                case OfferExpiredTargetType.Product:
                    if (message.ProductId.HasValue)
                    {
                        productsQuery = productsQuery.Where(p => p.Id == message.ProductId.Value);
                    }
                    break;

                case OfferExpiredTargetType.Category:
                    if (message.CategoryId.HasValue)
                    {
                        productsQuery = productsQuery.Where(p => p.CategoryId == message.CategoryId.Value);
                    }
                    break;

                case OfferExpiredTargetType.Occasion:
                    if (message.OccasionId.HasValue)
                    {
                        productsQuery = productsQuery
                            .Where(p => p.ProductOccasions.Any(po => po.OccasionId == message.OccasionId.Value));
                    }
                    break;

                case OfferExpiredTargetType.All:
                    // All products - no filter needed
                    break;
            }

            // Get products with this offer's ID
            var affectedProducts = await productsQuery
                .Where(p => p.ActiveOfferId == message.OfferId)
                .ToListAsync(context.CancellationToken);

            if (!affectedProducts.Any())
            {
                _logger.LogInformation(
                    "No products found with ActiveOfferId {OfferId}",
                    message.OfferId);
                return;
            }

            _logger.LogInformation(
                "Found {Count} products to reset prices for expired offer {OfferId}",
                affectedProducts.Count, message.OfferId);

            foreach (var product in affectedProducts)
            {
                // Reset discounted price and offer reference
                product.DiscountedPrice = null;
                product.ActiveOfferId = null;
                product.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Reset price for Product {ProductId}: {ProductName}. Original price: {Price}",
                    product.Id, product.Name, product.Price);
            }

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation(
                "Successfully reset prices for {Count} products from expired offer {OfferId}",
                affectedProducts.Count, message.OfferId);
        }
    }
}
