using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using Catalog_Service.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.OccasionsFeature.DeleteOccasion
{
    public class Handlers : IRequestHandler<DeleteOccasionCommand, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Occasion> _occasionRepository;
        private readonly IUnitOfWork _uow;
        private readonly ApplicationDbContext _context;

        public Handlers(
            IBaseRepository<Occasion> occasionRepository,
            IUnitOfWork uow,
            ApplicationDbContext context)
        {
            _occasionRepository = occasionRepository;
            _uow = uow;
            _context = context;
        }

        public async Task<RequestResponse<bool>> Handle(DeleteOccasionCommand request, CancellationToken cancellationToken)
        {
            var occasionToDelete = _occasionRepository
                .Get(o => o.Id == request.OccasionId && !o.IsDeleted)
                .FirstOrDefault();

            if (occasionToDelete is null)
                return RequestResponse<bool>.Fail("Occasion not found");

            // Check if any products are associated with this occasion
            // Query ProductOccasions directly since it's a join table without BaseEntity
            var hasAssociatedProducts = await _context.ProductOccasions
                .AnyAsync(po => po.OccasionId == request.OccasionId, cancellationToken);

            if (hasAssociatedProducts)
                return RequestResponse<bool>.Fail("Occasion has associated products and cannot be deleted");

            _occasionRepository.Delete(occasionToDelete);
            await _uow.SaveChangesAsync(cancellationToken);

            return RequestResponse<bool>.Success(true, "Occasion deleted successfully");
        }
    }
}
