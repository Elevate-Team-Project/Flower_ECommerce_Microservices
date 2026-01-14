using BuildingBlocks.SharedEntities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;

namespace Ordering_Service.Infrastructure.Data
{
    public class OrderingDbContext : DbContext
    {
        public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
        {
        }

        // =========================================================
        // 📦 DbSets
        // =========================================================
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DiscountUsage> DiscountUsages { get; set; }
        
        // Cart Service Merge
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // Delivery Service Merge
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<DeliveryZone> DeliveryZones { get; set; }

        //public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

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
            // 🔗 Relationships & Keys Configuration
            // =========================================================

            // 1. Configure Order -> OrderItems Relationship (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Configure Order -> DiscountUsage Relationship (One-to-One)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.DiscountUsage)
                .WithOne(d => d.Order)
                .HasForeignKey<DiscountUsage>(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Configure Decimal Precision for Money
            modelBuilder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DiscountUsage>()
                .Property(d => d.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DiscountUsage>()
                .Property(d => d.DiscountValue)
                .HasColumnType("decimal(18,2)");
                
            // =========================================================
            // MERGED CART CONFIGURATION
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
                
            modelBuilder.Entity<Cart>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CartItem>().HasQueryFilter(e => !e.IsDeleted);

            // =========================================================
            // DELIVERIES & SHIPMENTS (Merged from Delivery Service)
            // =========================================================
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.DeliveryAddress)
                .WithMany()
                .HasForeignKey(s => s.DeliveryAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeliveryZone>()
                .Property(d => d.ShippingCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UserAddress>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Shipment>().HasIndex(s => s.OrderId);
            modelBuilder.Entity<Shipment>().HasIndex(s => s.TrackingNumber);

            modelBuilder.Entity<UserAddress>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Shipment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<DeliveryZone>().HasQueryFilter(e => !e.IsDeleted);

            // =========================================================
            // 🗑️ Global Query Filter (Soft Delete)
            // =========================================================
            modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<DiscountUsage>().HasQueryFilter(e => !e.IsDeleted);
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
