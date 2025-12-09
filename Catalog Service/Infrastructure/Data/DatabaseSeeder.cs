using Microsoft.EntityFrameworkCore;
using Catalog_Service.Infrastructure.Data;
using Catalog_Service.Entities;
using BuildingBlocks.SharedEntities;

namespace Catalog_Service.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            var ctx = sp.GetRequiredService<ApplicationDbContext>();
            ctx.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            // 1. Seed Brands
            try
            {
                await SeedBrandsAsync(ctx);
            }
            catch (Exception ex)
            {
                // Log exception
                throw;
            }

            // 2. Seed Categories
            try
            {
                await SeedCategoriesAsync(ctx);
            }
            catch (Exception ex)
            {
                throw;
            }

            // 3. Seed Occasions
            try
            {
                await SeedOccasionsAsync(ctx);
            }
            catch (Exception ex)
            {
                throw;
            }

            // 4. Seed Products (Depends on Brands, Categories, Occasions)
            try
            {
                await SeedProductsAsync(ctx);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static async Task SeedBrandsAsync(ApplicationDbContext ctx)
        {
            if (await ctx.Brands.AnyAsync()) return;

            var brands = new List<Brand>
            {
                new() { Name = "Bloom & Wild", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=BW", IsActive = true },
                new() { Name = "Interflora", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=IF", IsActive = true },
                new() { Name = "The Bouqs Co.", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=TBC", IsActive = true },
                new() { Name = "UrbanStems", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=US", IsActive = true },
                new() { Name = "Farmgirl Flowers", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=FF", IsActive = true },
                new() { Name = "FTD Flowers", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=FTD", IsActive = true },
                new() { Name = "ProFlowers", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=PF", IsActive = true },
                new() { Name = "1-800-Flowers", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=1800", IsActive = true },
                new() { Name = "Teleflora", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=TF", IsActive = true },
                new() { Name = "Floom", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=FL", IsActive = true },
                new() { Name = "Local Artisan", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=LA", IsActive = true },
                new() { Name = "Venus Et Fleur", LogoUrl = "https://dummyimage.com/100x100/000/fff?text=VF", IsActive = true }
            };

            await ctx.Brands.AddRangeAsync(brands);
            await ctx.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext ctx)
        {
            if (await ctx.Categories.AnyAsync()) return;

            // 1. Root Categories
            var catFresh = new Category { Name = "Fresh Flowers", ImageUrl = "https://dummyimage.com/300/green", IsActive = true };
            var catPlants = new Category { Name = "Plants", ImageUrl = "https://dummyimage.com/300/green", IsActive = true };
            var catGifts = new Category { Name = "Gifts & Hampers", ImageUrl = "https://dummyimage.com/300/green", IsActive = true };
            var catDried = new Category { Name = "Dried Flowers", ImageUrl = "https://dummyimage.com/300/green", IsActive = true };

            await ctx.Categories.AddRangeAsync(catFresh, catPlants, catGifts, catDried);
            await ctx.SaveChangesAsync(); // Save to generate IDs for parents

            // 2. Sub Categories
            var subCategories = new List<Category>
            {
                // Fresh Flowers Sub
                new() { Name = "Roses", ParentCategoryId = catFresh.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Lilies", ParentCategoryId = catFresh.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Tulips", ParentCategoryId = catFresh.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Sunflowers", ParentCategoryId = catFresh.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Orchids", ParentCategoryId = catFresh.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Mixed Bouquets", ParentCategoryId = catFresh.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Peonies", ParentCategoryId = catFresh.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },

                // Plants Sub
                new() { Name = "Indoor Plants", ParentCategoryId = catPlants.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Succulents", ParentCategoryId = catPlants.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Bonsai Trees", ParentCategoryId = catPlants.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Office Plants", ParentCategoryId = catPlants.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },

                // Gifts Sub
                new() { Name = "Chocolates", ParentCategoryId = catGifts.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Teddy Bears", ParentCategoryId = catGifts.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Fruit Baskets", ParentCategoryId = catGifts.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true },
                new() { Name = "Luxury Hampers", ParentCategoryId = catGifts.Id, ImageUrl = "https://dummyimage.com/300", IsActive = true }
            };

            await ctx.Categories.AddRangeAsync(subCategories);
            await ctx.SaveChangesAsync();
        }

        private static async Task SeedOccasionsAsync(ApplicationDbContext ctx)
        {
            if (await ctx.Occasions.AnyAsync()) return;

            var occasions = new List<Occasion>
            {
                new() { Name = "Birthday", Description = "Celebrate another year around the sun.", ImageUrl = "img/occ/bday.jpg" },
                new() { Name = "Anniversary", Description = "Mark your special milestones.", ImageUrl = "img/occ/anniv.jpg" },
                new() { Name = "Wedding", Description = "Elegant flowers for the big day.", ImageUrl = "img/occ/wedding.jpg" },
                new() { Name = "Sympathy & Funeral", Description = "Express your condolences.", ImageUrl = "img/occ/sympathy.jpg" },
                new() { Name = "Get Well Soon", Description = "Brighten their day and recovery.", ImageUrl = "img/occ/getwell.jpg" },
                new() { Name = "New Baby", Description = "Welcome the little one.", ImageUrl = "img/occ/baby.jpg" },
                new() { Name = "Thank You", Description = "Show your gratitude.", ImageUrl = "img/occ/thanks.jpg" },
                new() { Name = "Romance", Description = "For the one you love.", ImageUrl = "img/occ/love.jpg" },
                new() { Name = "Valentine's Day", Description = "February 14th specials.", ImageUrl = "img/occ/val.jpg" },
                new() { Name = "Mother's Day", Description = "For the best mom in the world.", ImageUrl = "img/occ/mom.jpg" },
                new() { Name = "Christmas", Description = "Festive holiday arrangements.", ImageUrl = "img/occ/xmas.jpg" },
                new() { Name = "Graduation", Description = "Celebrate their success.", ImageUrl = "img/occ/grad.jpg" }
            };

            await ctx.Occasions.AddRangeAsync(occasions);
            await ctx.SaveChangesAsync();
        }

        private static async Task SeedProductsAsync(ApplicationDbContext ctx)
        {
            if (await ctx.Products.AnyAsync()) return;

            // Fetch dependencies
            var bBloom = await ctx.Brands.FirstAsync(b => b.Name == "Bloom & Wild");
            var bInter = await ctx.Brands.FirstAsync(b => b.Name == "Interflora");
            var bUrban = await ctx.Brands.FirstAsync(b => b.Name == "UrbanStems");
            var bLocal = await ctx.Brands.FirstAsync(b => b.Name == "Local Artisan");

            var cRoses = await ctx.Categories.FirstAsync(c => c.Name == "Roses");
            var cLilies = await ctx.Categories.FirstAsync(c => c.Name == "Lilies");
            var cTulips = await ctx.Categories.FirstAsync(c => c.Name == "Tulips");
            var cMixed = await ctx.Categories.FirstAsync(c => c.Name == "Mixed Bouquets");
            var cIndoor = await ctx.Categories.FirstAsync(c => c.Name == "Indoor Plants");
            var cChoco = await ctx.Categories.FirstAsync(c => c.Name == "Chocolates");

            var oBday = await ctx.Occasions.FirstAsync(o => o.Name == "Birthday");
            var oLove = await ctx.Occasions.FirstAsync(o => o.Name == "Romance");
            var oWed = await ctx.Occasions.FirstAsync(o => o.Name == "Wedding");
            var oSymp = await ctx.Occasions.FirstAsync(o => o.Name == "Sympathy & Funeral");

            var products = new List<Product>
            {
                // ==========================
                // 🌹 ROSES
                // ==========================
                new()
                {
                    Name = "The Red Romance",
                    Description = "A stunning bouquet of 24 premium long-stemmed red roses, hand-tied with gypsophila.",
                    Price = 59.99m,
                    StockQuantity = 100,
                    IsAvailable = true,
                    Brand = bInter,
                    Category = cRoses,
                    Images = new List<ProductImage> {
                        new() { ImageUrl = "https://dummyimage.com/600x600/ff0000/fff?text=Red+Roses+1", IsMain = true },
                        new() { ImageUrl = "https://dummyimage.com/600x600/ff0000/fff?text=Red+Roses+2", IsMain = false }
                    },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Count", SpecValue = "24" },
                        new() { SpecKey = "Color", SpecValue = "Red" },
                        new() { SpecKey = "Stem Length", SpecValue = "60cm" }
                    },
                    ProductOccasions = new List<ProductOccasion> {
                        new() { Occasion = oLove },
                        new() { Occasion = oWed }
                    },
                    PriceHistories = new List<PriceHistory> { // Initial price log
                        new() { NewPrice = 59.99m, OldPrice = 0, ChangeReason = "Initial Creation", CreatedAt = DateTime.UtcNow}
                    }
                },
                new()
                {
                    Name = "Pink Whisper",
                    Description = "Soft pink roses arranged with eucalyptus leaves for a modern, elegant look.",
                    Price = 45.00m,
                    StockQuantity = 50,
                    IsAvailable = true,
                    Brand = bBloom,
                    Category = cRoses,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/ffc0cb/000?text=Pink+Roses", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Count", SpecValue = "12" },
                        new() { SpecKey = "Color", SpecValue = "Pink" }
                    },
                    ProductOccasions = new List<ProductOccasion> {
                        new() { Occasion = oBday },
                        new() { Occasion = oLove }
                    }
                },
                new()
                {
                    Name = "White Elegance",
                    Description = "Pristine white roses symbolizing purity and innocence. Perfect for sympathy or weddings.",
                    Price = 55.00m,
                    StockQuantity = 80,
                    IsAvailable = true,
                    Brand = bLocal,
                    Category = cRoses,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/fff/000?text=White+Roses", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Count", SpecValue = "18" },
                        new() { SpecKey = "Color", SpecValue = "White" }
                    },
                    ProductOccasions = new List<ProductOccasion> {
                        new() { Occasion = oSymp },
                        new() { Occasion = oWed }
                    }
                },
                new()
                {
                    Name = "Yellow Friendship",
                    Description = "Bright yellow roses to bring sunshine to anyone's day.",
                    Price = 40.00m,
                    StockQuantity = 120,
                    IsAvailable = true,
                    Brand = bUrban,
                    Category = cRoses,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/ffff00/000?text=Yellow+Roses", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Count", SpecValue = "12" },
                        new() { SpecKey = "Color", SpecValue = "Yellow" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Rainbow Delight",
                    Description = "Dyed rainbow roses for a fun and colorful surprise.",
                    Price = 65.00m,
                    StockQuantity = 20,
                    IsAvailable = true,
                    Brand = bInter,
                    Category = cRoses,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/000/fff?text=Rainbow+Roses", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Count", SpecValue = "24" },
                        new() { SpecKey = "Color", SpecValue = "Multicolor" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },

                // ==========================
                // 🌷 TULIPS
                // ==========================
                new()
                {
                    Name = "Spring Awakening",
                    Description = "A vibrant mix of red, yellow, and purple tulips fresh from the fields.",
                    Price = 35.00m,
                    StockQuantity = 200,
                    IsAvailable = true,
                    Brand = bBloom,
                    Category = cTulips,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/000/fff?text=Mix+Tulips", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Count", SpecValue = "30" },
                        new() { SpecKey = "Season", SpecValue = "Spring" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Dutch Purple",
                    Description = "Deep purple tulips with a velvety texture.",
                    Price = 38.00m,
                    StockQuantity = 60,
                    IsAvailable = true,
                    Brand = bUrban,
                    Category = cTulips,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/800080/fff?text=Purple+Tulips", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Count", SpecValue = "20" },
                        new() { SpecKey = "Origin", SpecValue = "Holland" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oLove } }
                },

                // ==========================
                // 💐 MIXED BOUQUETS
                // ==========================
                new()
                {
                    Name = "The Elizabeth",
                    Description = "Our signature bouquet featuring lilies, roses, and lisianthus in soft pastels.",
                    Price = 75.00m,
                    StockQuantity = 40,
                    IsAvailable = true,
                    Brand = bBloom,
                    Category = cMixed,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/ffccaa/000?text=Luxury+Mix", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Arrangement Type", SpecValue = "Hand-tied" },
                        new() { SpecKey = "Vase Included", SpecValue = "No" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday }, new() { Occasion = oWed } }
                },
                new()
                {
                    Name = "Summer Sunshine",
                    Description = "Sunflowers mixed with solidago and green foliage.",
                    Price = 42.00m,
                    StockQuantity = 90,
                    IsAvailable = true,
                    Brand = bLocal,
                    Category = cMixed,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/ffff00/000?text=Sunflowers", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Dominant Flower", SpecValue = "Sunflower" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Winter Wonderland",
                    Description = "White roses, silver painted foliage, and pine cones.",
                    Price = 50.00m,
                    StockQuantity = 0, // Out of stock example
                    IsAvailable = false,
                    Brand = bInter,
                    Category = cMixed,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/ccc/000?text=Winter+Mix", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Season", SpecValue = "Winter" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oSymp } }
                },
                new()
                {
                    Name = "Wild Meadow",
                    Description = "A rustic arrangement of wildflowers, cornflowers, and daisies.",
                    Price = 48.00m,
                    StockQuantity = 30,
                    IsAvailable = true,
                    Brand = bBloom,
                    Category = cMixed,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/green/fff?text=Wild+Meadow", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Style", SpecValue = "Rustic" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },

                // ==========================
                // 🌿 INDOOR PLANTS
                // ==========================
                new()
                {
                    Name = "Peace Lily",
                    Description = "An elegant air-purifying plant with white blooms. Easy to care for.",
                    Price = 30.00m,
                    StockQuantity = 150,
                    IsAvailable = true,
                    Brand = bUrban,
                    Category = cIndoor,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/006400/fff?text=Peace+Lily", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Pot Size", SpecValue = "12cm" },
                        new() { SpecKey = "Light", SpecValue = "Low to Medium" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oSymp }, new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Fiddle Leaf Fig",
                    Description = "A trendy statement plant with large, violin-shaped leaves.",
                    Price = 85.00m,
                    StockQuantity = 20,
                    IsAvailable = true,
                    Brand = bUrban,
                    Category = cIndoor,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/006400/fff?text=Fiddle+Leaf", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Height", SpecValue = "80-100cm" },
                        new() { SpecKey = "Light", SpecValue = "Bright Indirect" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Snake Plant",
                    Description = "Indestructible and architectural. Perfect for beginners.",
                    Price = 25.00m,
                    StockQuantity = 200,
                    IsAvailable = true,
                    Brand = bLocal,
                    Category = cIndoor,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/006400/fff?text=Snake+Plant", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Toxicity", SpecValue = "Toxic to pets" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Bonsai Ficus",
                    Description = "A miniature tree symbolizing peace and harmony.",
                    Price = 45.00m,
                    StockQuantity = 40,
                    IsAvailable = true,
                    Brand = bInter,
                    Category = cIndoor,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/006400/fff?text=Bonsai", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Age", SpecValue = "5 Years" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Orchid - Phalaenopsis",
                    Description = "Double-stemmed white orchid in a ceramic pot.",
                    Price = 32.00m,
                    StockQuantity = 60,
                    IsAvailable = true,
                    Brand = bInter,
                    Category = cIndoor,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/fff/000?text=Orchid", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Color", SpecValue = "White" },
                        new() { SpecKey = "Blooms", SpecValue = "Long-lasting" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oWed }, new() { Occasion = oSymp } }
                },

                // ==========================
                // 🍫 CHOCOLATES & GIFTS
                // ==========================
                new()
                {
                    Name = "Luxury Truffle Box",
                    Description = "24 Assorted Belgian chocolate truffles.",
                    Price = 22.00m,
                    StockQuantity = 500,
                    IsAvailable = true,
                    Brand = bInter,
                    Category = cChoco,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/442200/fff?text=Chocolates", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Weight", SpecValue = "300g" },
                        new() { SpecKey = "Dietary", SpecValue = "Contains Nuts" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oLove }, new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Teddy Bear & Chocolates",
                    Description = "A cute 20cm plush bear with a small box of chocolates.",
                    Price = 25.00m,
                    StockQuantity = 100,
                    IsAvailable = true,
                    Brand = bLocal,
                    Category = cChoco,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/442200/fff?text=Teddy+Combo", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Bear Size", SpecValue = "20cm" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oLove } }
                },

                 // ==========================
                // 🌸 LILIES
                // ==========================
                new()
                {
                    Name = "Oriental Stargazer",
                    Description = "Fragrant pink and white Stargazer lilies arranged with greenery.",
                    Price = 48.00m,
                    StockQuantity = 70,
                    IsAvailable = true,
                    Brand = bBloom,
                    Category = cLilies,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/ff69b4/fff?text=Stargazer+Lilies", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Scent", SpecValue = "High" },
                        new() { SpecKey = "Stem Count", SpecValue = "10" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday }, new() { Occasion = oSymp } }
                },
                new()
                {
                    Name = "Pure White Lilies",
                    Description = "Elegant long-stemmed white lilies.",
                    Price = 52.00m,
                    StockQuantity = 50,
                    IsAvailable = true,
                    Brand = bInter,
                    Category = cLilies,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/fff/000?text=White+Lilies", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Scent", SpecValue = "Medium" },
                        new() { SpecKey = "Color", SpecValue = "White" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oWed }, new() { Occasion = oSymp } }
                },

                // ==========================
                // 🎁 HAMPERS
                // ==========================
                new()
                {
                    Name = "The Grand Celebration",
                    Description = "A wicker basket filled with wine, cheese, crackers, and chocolates.",
                    Price = 120.00m,
                    StockQuantity = 15,
                    IsAvailable = true,
                    Brand = bLocal,
                    Category = cChoco, // Or create a Hamper category
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/000/fff?text=Hamper", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Alcohol", SpecValue = "Yes" },
                        new() { SpecKey = "Weight", SpecValue = "3kg" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oWed }, new() { Occasion = oBday } }
                },

                // ==========================
                // 🌹 MORE ROSES (To add bulk)
                // ==========================
                new()
                {
                    Name = "Peach Perfect",
                    Description = "Subtle peach roses paired with hypericum berries.",
                    Price = 44.00m,
                    StockQuantity = 65,
                    IsAvailable = true,
                    Brand = bBloom,
                    Category = cRoses,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/ffdab9/000?text=Peach+Roses", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Color", SpecValue = "Peach" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                },
                new()
                {
                    Name = "Grand Prix Red",
                    Description = "The largest head size red rose available.",
                    Price = 80.00m,
                    StockQuantity = 30,
                    IsAvailable = true,
                    Brand = bInter,
                    Category = cRoses,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/8b0000/fff?text=Grand+Prix", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Stem Length", SpecValue = "80cm" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oLove } }
                },
                new()
                {
                    Name = "Lavender Dreams",
                    Description = "Rare lavender colored roses.",
                    Price = 58.00m,
                    StockQuantity = 40,
                    IsAvailable = true,
                    Brand = bUrban,
                    Category = cRoses,
                    Images = new List<ProductImage> { new() { ImageUrl = "https://dummyimage.com/600x600/e6e6fa/000?text=Lavender+Roses", IsMain = true } },
                    Specifications = new List<ProductSpecification> {
                        new() { SpecKey = "Color", SpecValue = "Lavender" }
                    },
                    ProductOccasions = new List<ProductOccasion> { new() { Occasion = oBday } }
                }
            };

            await ctx.Products.AddRangeAsync(products);
            await ctx.SaveChangesAsync();
        }
    }
}