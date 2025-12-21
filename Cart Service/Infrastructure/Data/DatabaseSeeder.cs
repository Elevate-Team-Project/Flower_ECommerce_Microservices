using Microsoft.EntityFrameworkCore;
using Cart_Service.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cart_Service.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            var ctx = sp.GetRequiredService<ApplicationDbContext>();
            ctx.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            try
            {
                await SeedCartsAsync(ctx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding failed: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedCartsAsync(ApplicationDbContext ctx)
        {
            if (await ctx.Carts.AnyAsync()) return;

            var carts = new List<Cart>();

            // =============================================================
            // 👤 USER 1: The "Romantic" Buyer (Flowers + Chocolates)
            // =============================================================
            carts.Add(new Cart
            {
                UserId = "user-romantic-01",
                UpdatedAt = DateTime.UtcNow,
                Items = new List<CartItem>
                {
                    new()
                    {
                        ProductId = 1,
                        ProductName = "The Red Romance",
                        UnitPrice = 59.99m,
                        Quantity = 1,
                        PictureUrl = "https://dummyimage.com/600x600/ff0000/fff?text=Red+Roses+1"
                    },
                    new()
                    {
                        ProductId = 5,
                        ProductName = "Luxury Truffle Box",
                        UnitPrice = 22.00m,
                        Quantity = 1,
                        PictureUrl = "https://dummyimage.com/600x600/442200/fff?text=Chocolates"
                    }
                }
            });

            // =============================================================
            // 👤 USER 2: The "Bulk" Buyer (Event Planner)
            // =============================================================
            carts.Add(new Cart
            {
                UserId = "user-planner-02",
                UpdatedAt = DateTime.UtcNow.AddHours(-2),
                Items = new List<CartItem>
                {
                    new()
                    {
                        ProductId = 3,
                        ProductName = "White Elegance",
                        UnitPrice = 55.00m,
                        Quantity = 10, // Bulk order for a wedding?
                        PictureUrl = "https://dummyimage.com/600x600/fff/000?text=White+Roses"
                    },
                    new()
                    {
                        ProductId = 7,
                        ProductName = "Oriental Stargazer",
                        UnitPrice = 48.00m,
                        Quantity = 5,
                        PictureUrl = "https://dummyimage.com/600x600/ff69b4/fff?text=Stargazer+Lilies"
                    }
                }
            });

            // =============================================================
            // 👤 USER 3: The "Plant Parent" (Indoor Plants)
            // =============================================================
            carts.Add(new Cart
            {
                UserId = "user-plantlover-03",
                UpdatedAt = DateTime.UtcNow.AddDays(-1), // Abandoned yesterday?
                Items = new List<CartItem>
                {
                    new()
                    {
                        ProductId = 12,
                        ProductName = "Fiddle Leaf Fig",
                        UnitPrice = 85.00m,
                        Quantity = 1,
                        PictureUrl = "https://dummyimage.com/600x600/006400/fff?text=Fiddle+Leaf"
                    },
                    new()
                    {
                        ProductId = 13,
                        ProductName = "Snake Plant",
                        UnitPrice = 25.00m,
                        Quantity = 2,
                        PictureUrl = "https://dummyimage.com/600x600/006400/fff?text=Snake+Plant"
                    }
                }
            });

            // =============================================================
            // 👤 USER 4: The "Gift Giver" (Teddy + Coupon)
            // =============================================================
            carts.Add(new Cart
            {
                UserId = "user-gifter-04",
                CouponCode = "WELCOME10", // Has a coupon applied
                UpdatedAt = DateTime.UtcNow,
                Items = new List<CartItem>
                {
                    new()
                    {
                        ProductId = 6,
                        ProductName = "Teddy Bear & Chocolates",
                        UnitPrice = 25.00m,
                        Quantity = 1,
                        PictureUrl = "https://dummyimage.com/600x600/442200/fff?text=Teddy+Combo"
                    },
                    new()
                    {
                        ProductId = 2,
                        ProductName = "Pink Whisper",
                        UnitPrice = 45.00m,
                        Quantity = 1,
                        PictureUrl = "https://dummyimage.com/600x600/ffc0cb/000?text=Pink+Roses"
                    }
                }
            });

            // =============================================================
            // 👤 USER 5: The "Single Item" Buyer (Just checking out)
            // =============================================================
            carts.Add(new Cart
            {
                UserId = "user-quick-05",
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                Items = new List<CartItem>
                {
                    new()
                    {
                        ProductId = 9,
                        ProductName = "The Grand Celebration",
                        UnitPrice = 120.00m,
                        Quantity = 1,
                        PictureUrl = "https://dummyimage.com/600x600/000/fff?text=Hamper"
                    }
                }
            });

            await ctx.Carts.AddRangeAsync(carts);
            await ctx.SaveChangesAsync();
        }
    }
}