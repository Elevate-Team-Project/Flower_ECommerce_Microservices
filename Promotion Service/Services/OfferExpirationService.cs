using BuildingBlocks.Interfaces;
using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Infrastructure;

namespace Promotion_Service.Services
{
    /// <summary>
    /// Background service that automatically expires offers past their end date.
    /// US-G01: Expired offers automatically deactivate.
    /// Runs every hour to check and update offer statuses.
    /// </summary>
    public class OfferExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OfferExpirationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public OfferExpirationService(
            IServiceProvider serviceProvider,
            ILogger<OfferExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OfferExpirationService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredOffersAsync(stoppingToken);
                    await ProcessScheduledOffersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing offers in OfferExpirationService");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Find offers past their EndDate and mark them as Expired.
        /// </summary>
        private async Task ProcessExpiredOffersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var offerRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Offer>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var now = DateTime.UtcNow;

            // Find active or scheduled offers that have expired
            var expiredOffers = await offerRepository
                .GetAll()
                .Where(o => o.EndDate < now && 
                           (o.Status == OfferStatus.Active || o.Status == OfferStatus.Scheduled))
                .ToListAsync(cancellationToken);

            if (!expiredOffers.Any())
                return;

            _logger.LogInformation("Found {Count} offers to expire", expiredOffers.Count);

            foreach (var offer in expiredOffers)
            {
                offer.Status = OfferStatus.Expired;
                offer.UpdatedAt = now;
                offerRepository.Update(offer);

                // Publish event for Catalog Service to reset product prices
                var expiredEvent = new OfferExpiredEvent(
                    offer.Id,
                    offer.Name,
                    MapTargetType(offer.TargetType),
                    offer.ProductId,
                    offer.CategoryId,
                    offer.OccasionId,
                    now
                );

                await publishEndpoint.Publish(expiredEvent, cancellationToken);

                _logger.LogInformation(
                    "Expired offer {OfferId}: {OfferName}",
                    offer.Id, offer.Name);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully expired {Count} offers", expiredOffers.Count);
        }

        /// <summary>
        /// Find scheduled offers that should now be active.
        /// </summary>
        private async Task ProcessScheduledOffersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var offerRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Offer>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var now = DateTime.UtcNow;

            // Find scheduled offers that should now be active
            var scheduledOffers = await offerRepository
                .GetAll()
                .Where(o => o.Status == OfferStatus.Scheduled && 
                           o.StartDate <= now && 
                           o.EndDate > now)
                .ToListAsync(cancellationToken);

            if (!scheduledOffers.Any())
                return;

            _logger.LogInformation("Found {Count} scheduled offers to activate", scheduledOffers.Count);

            foreach (var offer in scheduledOffers)
            {
                offer.Status = OfferStatus.Active;
                offer.UpdatedAt = now;
                offerRepository.Update(offer);

                _logger.LogInformation(
                    "Activated scheduled offer {OfferId}: {OfferName}",
                    offer.Id, offer.Name);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static OfferExpiredTargetType MapTargetType(OfferTargetType targetType)
        {
            return targetType switch
            {
                OfferTargetType.Product => OfferExpiredTargetType.Product,
                OfferTargetType.Category => OfferExpiredTargetType.Category,
                OfferTargetType.Occasion => OfferExpiredTargetType.Occasion,
                OfferTargetType.All => OfferExpiredTargetType.All,
                _ => OfferExpiredTargetType.Product
            };
        }
    }
}
