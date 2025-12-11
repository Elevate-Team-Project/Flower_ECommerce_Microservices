using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus
{
    public class ActivateCategoryHandler : IRequestHandler<ActivateCategoryCommand, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Category> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateCategoryHandler(IBaseRepository<Category> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<bool>> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repo.GetByIdAsync(request.Id);
            if (category == null)
                return RequestResponse<bool>.Fail("Category not found");

            category.IsActive = true;
            category.UpdatedAt = DateTime.UtcNow;

            _repo.SaveInclude(
                category,
                nameof(Category.IsActive),
                nameof(Category.UpdatedAt)
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return RequestResponse<bool>.Success(true, "Category activated successfully");
        }
    }
}
