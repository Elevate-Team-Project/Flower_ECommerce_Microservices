using BuildingBlocks.Interfaces;

namespace Promotion_Service.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PromotionDbContext _context;

        public UnitOfWork(PromotionDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
