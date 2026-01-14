using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Notification_Service.Services;

namespace Notification_Service.Features.Inventory
{
    public class ProductLowStockConsumer : IConsumer<ProductLowStockEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ProductLowStockConsumer> _logger;

        public ProductLowStockConsumer(IEmailSender emailSender, ILogger<ProductLowStockConsumer> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductLowStockEvent> context)
        {
            var message = context.Message;
            _logger.LogWarning("Received Low Stock Alert for Product: {ProductName} (ID: {ProductId}). Current Stock: {Stock}", message.ProductName, message.ProductId, message.CurrentStock);

            await _emailSender.SendEmailAsync(
                "admin@flowerecommerce.com", 
                $"URGENT: Low Stock Alert - {message.ProductName}",
                $"The product '{message.ProductName}' (ID: {message.ProductId}) is running low on stock.\nCurrent Stock: {message.CurrentStock}\nMin Stock: {message.MinStock}\n\nPlease restock immediately."
            );
        }
    }
}
