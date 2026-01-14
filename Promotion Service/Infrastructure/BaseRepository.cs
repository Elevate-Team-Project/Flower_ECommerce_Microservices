using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Promotion_Service.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly PromotionDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(PromotionDbContext context)
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
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).Where(e => !e.IsDeleted);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.Where(e => !e.IsDeleted);
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
            entity.UpdatedAt = DateTime.UtcNow;
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
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public void HardDelete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
            }
            _dbSet.UpdateRange(entities);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? criteria = null)
        {
            if (criteria == null)
                return await _dbSet.Where(e => !e.IsDeleted).CountAsync();

            return await _dbSet.Where(e => !e.IsDeleted).CountAsync(criteria);
        }
    }
}
