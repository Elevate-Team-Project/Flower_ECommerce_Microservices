using BuildingBlocks.SharedEntities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;

namespace Delivery_Service.Infrastructure.Data
{
    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<DeliveryZone> DeliveryZones { get; set; }
        public DbSet<DeliveryAddress>deliveryAddress { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MassTransit Outbox
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            // Shipment -> UserAddress
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.DeliveryAddress)
                .WithMany()
                .HasForeignKey(s => s.DeliveryAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal Precision
            modelBuilder.Entity<DeliveryZone>()
                .Property(d => d.ShippingCost)
                .HasColumnType("decimal(18,2)");

            // Indexes
            modelBuilder.Entity<UserAddress>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Shipment>().HasIndex(s => s.OrderId);
            modelBuilder.Entity<Shipment>().HasIndex(s => s.TrackingNumber);

            // Soft Delete Filters
            modelBuilder.Entity<UserAddress>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Shipment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<DeliveryZone>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
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
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
