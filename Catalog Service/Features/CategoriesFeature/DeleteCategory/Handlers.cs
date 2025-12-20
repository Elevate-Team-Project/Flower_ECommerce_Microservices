 using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.DeleteCategory
{
    public class Handlers : IRequestHandler<DeleteCategoryCommand, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _uow;
        private readonly IBaseRepository<Product> _productRepository;

        public Handlers(
            IBaseRepository<Category> categoryRepository,
            IUnitOfWork uow,
            IBaseRepository<Product> productRepository)
        {
            _categoryRepository = categoryRepository;
            _uow = uow;
            _productRepository = productRepository;
        }

        public async Task<RequestResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var categoryToDelete = _categoryRepository
                .Get(c => c.Id == request.CategoryId && !c.IsDeleted)
                .FirstOrDefault();

            if (categoryToDelete is null)
                return RequestResponse<bool>.Fail("Category not found");

            var hasProducts = _productRepository
                .Get(p => p.CategoryId == request.CategoryId && !p.IsDeleted)
                .Any();

            if (hasProducts)
                return RequestResponse<bool>.Fail("Category has associated products and cannot be deleted");

            var category = new Category
            {
                Id = categoryToDelete.Id,              
                IsDeleted = true,
                DeletedAt= DateTime.UtcNow
            };
            _categoryRepository.SaveInclude(category, [nameof(category.IsDeleted),nameof(category.DeletedAt)]);

            await _uow.SaveChangesAsync(cancellationToken);

            return RequestResponse<bool>.Success(true, "Category deleted successfully");
        }
    }
}
