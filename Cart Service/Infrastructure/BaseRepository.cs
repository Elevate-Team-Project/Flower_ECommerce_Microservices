using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities;
using Cart_Service.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cart_Service.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Implement IBaseRepository methods
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
            var LocalEntity = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
            EntityEntry entry;

            if (LocalEntity == null)
            {
                entry = _context.Entry(entity);
            }
            else
            {
                entry = _context.ChangeTracker.Entries<T>().First(e => e.Entity.Id == entity.Id);
            }

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;
                else
                {
                    if (includedProperties.Contains(property.Metadata.Name))
                    {
                        property.IsModified = true;
                    }
                    else
                    {
                        property.IsModified = false;
                    }
                }
            }
        }

        // Soft Delete - marks as deleted but keeps in database
        public void Delete(T entity)
        {
            _dbSet.Attach(entity);
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            // Mark the entity as modified so EF will update it
            _context.Entry(entity).State = EntityState.Modified;
        }

        // Hard Delete - physically removes from database
        public void HardDelete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public void DeleteRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity); // Use soft delete for range as well
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
            Delete(entity); // Use soft delete
            return Task.CompletedTask;
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
    }
}
