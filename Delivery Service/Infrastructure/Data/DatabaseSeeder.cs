using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;

namespace Delivery_Service.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(DeliveryDbContext context)
        {
            try
            {
                // Check if there are pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                }

                // Only seed if tables exist and are empty
                if (!await context.Database.CanConnectAsync()) return;

                // Check if any data exists
                var hasData = await context.DeliveryZones.AnyAsync();
                if (hasData) return;

                var deliveryZones = new List<DeliveryZone>
                {
                    new() { ZoneName = "Local", City = "Cairo", State = "Cairo", Country = "Egypt", ShippingCost = 25.00m, EstimatedDeliveryDays = 1, IsActive = true },
                    new() { ZoneName = "Greater Cairo", City = "Giza", State = "Giza", Country = "Egypt", ShippingCost = 35.00m, EstimatedDeliveryDays = 1, IsActive = true },
                    new() { ZoneName = "Alexandria", City = "Alexandria", State = "Alexandria", Country = "Egypt", ShippingCost = 50.00m, EstimatedDeliveryDays = 2, IsActive = true }
                };

                await context.DeliveryZones.AddRangeAsync(deliveryZones);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error but don't crash - migrations may need to be created first
                Console.WriteLine($"[DatabaseSeeder] Warning: Could not seed database. Run migrations first. Error: {ex.Message}");
            }
        }
    }
}
