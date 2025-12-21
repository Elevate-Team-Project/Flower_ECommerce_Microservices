using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;

namespace Ordering_Service.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(OrderingDbContext context)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Check if we already have data
            if (await context.Orders.AnyAsync())
            {
                return; // Database already seeded
            }

            // Seed sample orders
            var sampleOrders = new List<Order>
            {
                new Order
                {
                    UserId = "sample-user-1",
                    SubTotal = 99.99m,
                    DiscountAmount = 0m,
                    ShippingCost = 5.99m,
                    TotalAmount = 105.98m,
                    Status = "Delivered",
                    ShippingAddress = "123 Flower St, Garden City, GC 12345",
                    BillingAddress = "123 Flower St, Garden City, GC 12345",
                    PaymentMethod = "Credit Card",
                    PaymentTransactionId = "TXN-001",
                    PaidAt = DateTime.UtcNow.AddDays(-7),
                    ShippedAt = DateTime.UtcNow.AddDays(-5),
                    DeliveredAt = DateTime.UtcNow.AddDays(-3),
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = 1,
                            ProductName = "Red Rose Bouquet",
                            ProductImageUrl = "/images/red-roses.jpg",
                            UnitPrice = 49.99m,
                            Quantity = 2
                        }
                    },
                    Shipments = new List<Shipment>
                    {
                        new Shipment
                        {
                            TrackingNumber = "TRACK-001-ABC",
                            Carrier = "FlowerExpress",
                            Status = "Delivered",
                            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(-3),
                            ActualDeliveryDate = DateTime.UtcNow.AddDays(-3),
                            CurrentLocation = "Delivered"
                        }
                    }
                },
                new Order
                {
                    UserId = "sample-user-2",
                    SubTotal = 149.99m,
                    DiscountAmount = 15.00m,
                    ShippingCost = 0m,
                    TotalAmount = 134.99m,
                    Status = "Processing",
                    ShippingAddress = "456 Bloom Ave, Petal Town, PT 67890",
                    BillingAddress = "456 Bloom Ave, Petal Town, PT 67890",
                    PaymentMethod = "PayPal",
                    PaymentTransactionId = "TXN-002",
                    PaidAt = DateTime.UtcNow.AddDays(-1),
                    CouponCode = "SPRING15",
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = 2,
                            ProductName = "Spring Collection Box",
                            ProductImageUrl = "/images/spring-box.jpg",
                            UnitPrice = 74.99m,
                            Quantity = 1
                        },
                        new OrderItem
                        {
                            ProductId = 3,
                            ProductName = "Lavender Bundle",
                            ProductImageUrl = "/images/lavender.jpg",
                            UnitPrice = 75.00m,
                            Quantity = 1
                        }
                    }
                }
            };

            await context.Orders.AddRangeAsync(sampleOrders);
            await context.SaveChangesAsync();
        }
    }
}
