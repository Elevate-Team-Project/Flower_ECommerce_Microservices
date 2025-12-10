using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus
{
    public class DeactivateCategoryHandler : IRequestHandler<DeactivateCategoryCommand, bool>
    {
        private readonly IBaseRepository<Category> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateCategoryHandler(IBaseRepository<Category> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repo.GetByIdAsync(request.Id);
            if (category == null)
                throw new Exception("Category not found");

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;

            _repo.SaveInclude(
                category,
                nameof(Category.IsActive),
                nameof(Category.UpdatedAt)
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
