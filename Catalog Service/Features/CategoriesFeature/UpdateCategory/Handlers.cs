using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategory
{
    public class Handlers : IRequestHandler<UpdateCategoryCommand, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handlers(IBaseRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.Dto;

                var category = await _categoryRepository.GetByIdAsync(request.Id);
                if (category == null)
                    return RequestResponse<bool>.Fail("Category not found");

                var exists = _categoryRepository
                    .Get(c => c.Name == dto.Name && c.Id != request.Id)
                    .Any();

                if (exists)
                    return RequestResponse<bool>.Fail("Category name already exists");

                category.Name = dto.Name;
                category.ImageUrl = dto.ImageUrl;
                category.ParentCategoryId = dto.ParentCategoryId;
                category.UpdatedAt = DateTime.UtcNow;

                _categoryRepository.SaveInclude(
                    category,
                    nameof(Category.Name),
                    nameof(Category.ImageUrl),
                    nameof(Category.ParentCategoryId),
                    nameof(Category.UpdatedAt)
                );

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return RequestResponse<bool>.Success(true, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return RequestResponse<bool>.Fail(ex.Message);
            }
        }
    }
}
