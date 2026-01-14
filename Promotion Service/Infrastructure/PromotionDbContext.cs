using MassTransit;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;

namespace Promotion_Service.Infrastructure
{
    public class PromotionDbContext : DbContext
    {
        public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options)
        {
        }

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

            // MassTransit Outbox Configuration
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
