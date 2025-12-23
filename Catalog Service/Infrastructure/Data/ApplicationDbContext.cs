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