using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Notification_Service.Entities;
using Notification_Service.Infrastructure;

namespace Notification_Service.Features.Notifications.Consumers
{
    /// <summary>
    /// Creates notifications when new offers are created
    /// </summary>
    public class OfferCreatedConsumer : IConsumer<OfferCreatedEvent>
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<OfferCreatedConsumer> _logger;

        public OfferCreatedConsumer(
            NotificationDbContext context,
            ILogger<OfferCreatedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OfferCreatedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Creating notifications for new offer {OfferId}: {OfferName}",
                message.OfferId, message.OfferName);

            // For new offers, we create a broadcast notification
            // In production, this would target all active users or subscribed users
            var notification = new Notification
            {
                UserId = "broadcast", // Special user ID for broadcast notifications
                Title = "New Offer Available! 🎉",
                Description = $"{message.OfferName}: {GetDiscountText(message)}. Valid until {message.EndDate:MMM dd, yyyy}",
                Type = "Offer",
                ReferenceId = message.OfferId.ToString(),
                ActionUrl = $"/offers/{message.OfferId}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private static string GetDiscountText(OfferCreatedEvent offer)
        {
            if (offer.DiscountPercentage.HasValue)
                return $"Save {offer.DiscountPercentage}% off";
            return $"Save {offer.DiscountAmount:C}";
        }
    }
}
