using BuildingBlocks.SharedEntities;
using Cart_Service.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
namespace Cart_Service.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // =========================================================
        // 📦 DbSets
        // =========================================================
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // 🚌 MassTransit Outbox Configuration
            // =========================================================
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
            // =========================================================

            // =========================================================
            // 🔗 Relationships & Keys Configuration
            // =========================================================

            // 1. Configure Cart Relationship (One-to-Many)
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade); // Important: Deleting a Cart deletes its items

            // 2. Configure Decimal Precision for Money
            modelBuilder.Entity<CartItem>()
                .Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // =========================================================
            // 🗑️ Global Query Filter (Soft Delete)
            // =========================================================
            modelBuilder.Entity<Cart>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CartItem>().HasQueryFilter(e => !e.IsDeleted);
        }

        // =========================================================
        // 💾 SaveChanges Interceptor (Automatic Auditing)
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
                            // Intercept delete and turn it into Soft Delete
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