using BuildingBlocks.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Payment_Service.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly PaymentDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(PaymentDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public async Task<T?> GetFirst(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
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
            var entry = _context.Entry(entity);
            entry.State = EntityState.Unchanged;
            
            foreach (var property in includedProperties)
            {
                entry.Property(property).IsModified = true;
            }
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void HardDelete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? criteria = null)
        {
            if (criteria == null)
                return await _dbSet.CountAsync();
            
            return await _dbSet.CountAsync(criteria);
        }
    }
}
