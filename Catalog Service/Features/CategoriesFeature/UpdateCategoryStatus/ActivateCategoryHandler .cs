using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus
{
    public class ActivateCategoryHandler : IRequestHandler<ActivateCategoryCommand, bool>
    {
        private readonly IBaseRepository<Category> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateCategoryHandler(IBaseRepository<Category> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repo.GetByIdAsync(request.Id);
            if (category == null)
                throw new Exception("Category not found");

            category.IsActive = true;
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
