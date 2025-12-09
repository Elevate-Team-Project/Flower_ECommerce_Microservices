using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using BuildingBlocks.SharedEntities; // Assuming BaseEntity is here
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Catalog_Service.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // =========================================================
        // 📦 DbSets (Based on your Solution Explorer)
        // =========================================================
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Occasion> Occasions { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }

        // Join Table
        public DbSet<ProductOccasion> ProductOccasions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // 🔗 Relationships & Keys Configuration
            // =========================================================

            // 1. Configure ProductOccasion (Many-to-Many Join Table)
            // Defining Composite Key
            modelBuilder.Entity<ProductOccasion>()
                .HasKey(po => new { po.ProductId, po.OccasionId });

            modelBuilder.Entity<ProductOccasion>()
                .HasOne(po => po.Product)
                .WithMany(p => p.ProductOccasions)
                .HasForeignKey(po => po.ProductId);

            modelBuilder.Entity<ProductOccasion>()
                .HasOne(po => po.Occasion)
                .WithMany(o => o.ProductOccasions)
                .HasForeignKey(po => po.OccasionId);

            // 2. Configure Category (Self-Referencing Relationship)
            // Handling Parent and Sub-categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a parent if it has children

            // 3. Configure Decimal Precision (Best Practice for Prices)
            // Explicitly defining precision to avoid EF Core warnings
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PriceHistory>()
                .Property(ph => ph.OldPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PriceHistory>()
                .Property(ph => ph.NewPrice)
                .HasColumnType("decimal(18,2)");

            // =========================================================
            // 🗑️ Global Query Filter (Soft Delete)
            // =========================================================
            // Automatically exclude deleted items (IsDeleted = true) from all queries
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Brand>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Occasion>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ProductImage>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ProductReview>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PriceHistory>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<StockAlert>().HasQueryFilter(e => !e.IsDeleted);

            // Note: Join tables like ProductOccasion usually don't inherit BaseEntity.
            // If you made them inherit BaseEntity, uncomment the line below:
            // modelBuilder.Entity<ProductOccasion>().HasQueryFilter(e => !e.IsDeleted);
        }

        // =========================================================
        // 💾 SaveChanges Interceptor (For Auditing & Soft Delete)
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
                // Logic applies to any class inheriting from BaseEntity
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
                            entry.State = EntityState.Modified; // Convert Hard Delete to Update
                            baseEntity.IsDeleted = true;        // Soft Delete
                            baseEntity.DeletedAt = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }
    }
}