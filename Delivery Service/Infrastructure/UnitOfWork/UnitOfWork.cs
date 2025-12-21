using BuildingBlocks.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Delivery_Service.Infrastructure.Data;

namespace Delivery_Service.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DeliveryDbContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(DeliveryDbContext context) => _context = context;

        public async Task BeginTransactionAsync() 
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            try { await _transaction.CommitAsync(); }
            finally { await _transaction.DisposeAsync(); _transaction = null; }
        }

        public async Task RollbackTransactionAsync()
        {
            try { await _transaction.RollbackAsync(); }
            finally { await _transaction.DisposeAsync(); _transaction = null; }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) 
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
