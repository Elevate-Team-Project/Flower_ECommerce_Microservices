 using BuildingBlocks.SharedEntities;
using Catalog_Service.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace Catalog_Service.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // =========================================================
        // 📦 DbSets
        // =========================================================
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Occasion> Occasions { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }

        // Join Table
        public DbSet<ProductOccasion> ProductOccasions { get; set; }

        // =========================================================
        // 🚀 PROMOTION MERGE: DbSets
        // =========================================================
        
        // Offers
        public DbSet<Offer> Offers { get; set; }

        // Coupons
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }

        // Loyalty
        public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
        public DbSet<LoyaltyTier> LoyaltyTiers { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

        // Registration Codes
        public DbSet<RegistrationCode> RegistrationCodes { get; set; }
        public DbSet<RegistrationCodeUsage> RegistrationCodeUsages { get; set; }

        // Banners
        public DbSet<Banner> Banners { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // =========================================================
            // 🚌 MassTransit Outbox Configuration (ADD THIS BLOCK)
            // =========================================================
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
            // =========================================================

            // =========================================================
            // 🔗 Relationships & Keys Configuration
            // =========================================================

            // 1. Configure ProductOccasion (Many-to-Many Join Table)
            modelBuilder.Entity<ProductOccasion>()
                .HasKey(po => new { po.ProductId, po.OccasionId });

            modelBuilder.Entity<ProductOccasion>()
                .HasOne(po => po.Product)
                .WithMany(p => p.ProductOccasions)
                .HasForeignKey(po => po.ProductId)
                .IsRequired(false); // ✅ FIX: Allow navigation to be null if Product is Soft Deleted

            modelBuilder.Entity<ProductOccasion>()
                .HasOne(po => po.Occasion)
                .WithMany(o => o.ProductOccasions)
                .HasForeignKey(po => po.OccasionId)
                .IsRequired(false); // ✅ FIX: Allow navigation to be null if Occasion is Soft Deleted

            // 2. Configure Category (Self-Referencing)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Configure ProductSpecification (1-to-Many)
            // ✅ FIX: Explicitly configure this to handle the relationship warning
            modelBuilder.Entity<ProductSpecification>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.Specifications)
                .HasForeignKey(ps => ps.ProductId);

            // 4. Configure Decimal Precision
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PriceHistory>().Property(ph => ph.OldPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PriceHistory>().Property(ph => ph.NewPrice).HasColumnType("decimal(18,2)");

            // =========================================================
            // 🚀 PROMOTION MERGE: Entity Configurations
            // =========================================================

            // Offer Configuration
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.StartDate, e.EndDate });
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.OccasionId);
                entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
                entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            });

            // Coupon Configuration
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => new { e.ValidFrom, e.ValidUntil });
                entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
                entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
            });

            // CouponUsage Configuration
            modelBuilder.Entity<CouponUsage>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.OrderId);
                entity.Property(e => e.DiscountApplied).HasPrecision(18, 2);
                entity.HasOne(e => e.Coupon)
                    .WithMany(c => c.Usages)
                    .HasForeignKey(e => e.CouponId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LoyaltyAccount Configuration
            modelBuilder.Entity<LoyaltyAccount>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne(e => e.Tier)
                    .WithMany(t => t.Accounts)
                    .HasForeignKey(e => e.TierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LoyaltyTier Configuration
            modelBuilder.Entity<LoyaltyTier>(entity =>
            {
                entity.HasIndex(e => e.MinPoints);
                entity.Property(e => e.PointsMultiplier).HasPrecision(5, 2);
                entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            });

            // LoyaltyTransaction Configuration
            modelBuilder.Entity<LoyaltyTransaction>(entity =>
            {
                entity.HasIndex(e => e.LoyaltyAccountId);
                entity.HasIndex(e => e.OrderId);
                entity.Property(e => e.OrderAmount).HasPrecision(18, 2);
                entity.HasOne(e => e.Account)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(e => e.LoyaltyAccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RegistrationCode Configuration
            modelBuilder.Entity<RegistrationCode>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.WelcomeCreditAmount).HasPrecision(18, 2);
            });

            // RegistrationCodeUsage Configuration
            modelBuilder.Entity<RegistrationCodeUsage>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.CreditAmount).HasPrecision(18, 2);
                entity.HasOne(e => e.RegistrationCode)
                    .WithMany(r => r.Usages)
                    .HasForeignKey(e => e.RegistrationCodeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Banner Configuration
            modelBuilder.Entity<Banner>(entity =>
            {
                entity.HasIndex(e => e.Position);
                entity.HasIndex(e => new { e.ValidFrom, e.ValidUntil });
                entity.HasIndex(e => e.SortOrder);
            });

            // =========================================================
            // 🗑️ Global Query Filter (Soft Delete)
            // =========================================================

            // Entities with Soft Delete
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Brand>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Occasion>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ProductImage>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ProductReview>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PriceHistory>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<StockAlert>().HasQueryFilter(e => !e.IsDeleted);

            // ✅ FIX: Added ProductSpecification to Query Filter
            // This resolves the warning for Product <-> ProductSpecification
            modelBuilder.Entity<ProductSpecification>().HasQueryFilter(e => !e.IsDeleted);

            // 🚀 PROMOTION MERGE: Global Query Filters (Soft Delete)
            modelBuilder.Entity<Offer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Coupon>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CouponUsage>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<LoyaltyAccount>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<LoyaltyTier>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<LoyaltyTransaction>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RegistrationCode>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RegistrationCodeUsage>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Banner>().HasQueryFilter(e => !e.IsDeleted);
        }

        // =========================================================
        // 💾 SaveChanges Interceptor
        // =========================================================
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity baseEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            baseEntity.CreatedAt = DateTime.UtcNow;
                            baseEntity.IsDeleted = false;
                            break;

                        case EntityState.Modified:
                            baseEntity.UpdatedAt = DateTime.UtcNow;
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            baseEntity.IsDeleted = true;
                            baseEntity.DeletedAt = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }
    }
}