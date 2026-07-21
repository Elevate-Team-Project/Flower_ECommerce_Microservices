using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Delivery_Service.Infrastructure.Data;
using System.Linq.Expressions;

namespace Delivery_Service.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly DeliveryDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(DeliveryDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => Microsoft.EntityFrameworkCore.EF.Property<int>(e, "Id") == id);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void SaveInclude(T entity, params string[] includedProperties)
        {
            var local = _dbSet.Local
                .FirstOrDefault(e => _context.Entry(e).Property("Id").CurrentValue!
                                     .Equals(_context.Entry(entity).Property("Id").CurrentValue));

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> entry;

            if (local == null)
            {
                entry = _context.Attach(entity);
            }
            else
            {
                entry = _context.Entry(local);
            }

            foreach (var property in includedProperties)
            {
                entry.Property(property).IsModified = true;
            }
        }

        public void Delete(T entity)
        {
            _dbSet.Attach(entity);
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void HardDelete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? criteria = null)
        {
            if (criteria == null)
            {
                return await _dbSet.CountAsync();
            }

            return await _dbSet.CountAsync(criteria);
        }

        public Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            Delete(entity);
            return Task.CompletedTask;
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
    }
}
