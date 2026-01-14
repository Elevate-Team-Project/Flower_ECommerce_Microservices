using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Notification_Service.Entities;
using Notification_Service.Infrastructure;

namespace Notification_Service.Features.Notifications.Consumers
{
    /// <summary>
    /// Creates notifications when order status changes
    /// </summary>
    public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<OrderStatusChangedConsumer> _logger;

        public OrderStatusChangedConsumer(
            NotificationDbContext context,
            ILogger<OrderStatusChangedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Creating notification for order {OrderId} status change to {Status}",
                message.OrderId, message.NewStatus);

            var notification = new Notification
            {
                UserId = message.UserId,
                Title = GetNotificationTitle(message.NewStatus),
                Description = GetNotificationDescription(message),
                Type = "Order",
                ReferenceId = message.OrderId.ToString(),
                ActionUrl = $"/orders/{message.OrderId}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private static string GetNotificationTitle(string status) => status switch
        {
            "Received" => "Order Received",
            "Preparing" => "Your order is being prepared",
            "OutForDelivery" => "Your order is out for delivery!",
            "Delivered" => "Order Delivered!",
            _ => "Order Status Update"
        };

        private static string GetNotificationDescription(OrderStatusChangedEvent evt) => evt.NewStatus switch
        {
            "Received" => $"We've received your order #{evt.OrderId}. Thank you for shopping with us!",
            "Preparing" => $"Good news! Order #{evt.OrderId} is now being prepared.",
            "OutForDelivery" => $"Your order #{evt.OrderId} is on its way! Track your delivery in real-time.",
            "Delivered" => $"Your order #{evt.OrderId} has been delivered. Enjoy!",
            _ => $"Order #{evt.OrderId} status changed to {evt.NewStatus}"
        };
    }
}
