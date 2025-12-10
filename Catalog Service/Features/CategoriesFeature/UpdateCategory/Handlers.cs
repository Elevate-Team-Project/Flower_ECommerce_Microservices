using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategory
{
    public class Handlers : IRequestHandler<UpdateCategoryCommand, UpdateCategoryDto>
    {
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public Handlers(IBaseRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateCategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
                throw new Exception("Category not found");

         
            var exists = _categoryRepository
                .Get(c => c.Name == dto.Name && c.Id != request.Id)
                .Any();

            if (exists)
                throw new Exception("Category name already exists");

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

            return new UpdateCategoryDto
            {
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId
            };
        }
    }
}
