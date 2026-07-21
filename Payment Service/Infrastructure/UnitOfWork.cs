using BuildingBlocks.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Payment_Service.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentDbContext _context;
        private int _depth = 0;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            if (_depth == 0 && _transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }

            _depth++;

            try
            {
                await action();

                _depth--;

                if (_depth == 0)
                {
                    await _context.SaveChangesAsync();
                    await _transaction!.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch
            {
                _depth--;
                if (_depth == 0 && _transaction != null)
                {
                    await _transaction.RollbackAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
                throw;
            }
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            if (_depth == 0 && _transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }

            _depth++;

            try
            {
                var result = await action();

                _depth--;

                if (_depth == 0)
                {
                    await _context.SaveChangesAsync();
                    await _transaction!.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }

                return result;
            }
            catch
            {
                _depth--;
                if (_depth == 0 && _transaction != null)
                {
                    await _transaction.RollbackAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
                throw;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.CommitAsync();
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.RollbackAsync();
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
