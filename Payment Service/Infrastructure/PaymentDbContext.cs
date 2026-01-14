using Microsoft.EntityFrameworkCore;
using Payment_Service.Entities;
using MassTransit;

namespace Payment_Service.Infrastructure
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.Property(p => p.Amount)
                    .HasPrecision(18, 2);
                
                entity.Property(p => p.RefundedAmount)
                    .HasPrecision(18, 2);

                entity.HasIndex(p => p.OrderId);
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.StripePaymentIntentId);
                entity.HasIndex(p => p.Status);

                // Soft delete filter
                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            // MassTransit Outbox tables
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
