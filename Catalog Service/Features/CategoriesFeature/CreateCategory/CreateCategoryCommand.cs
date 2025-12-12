//using BuildingBlocks.FullEntities.Catalog_Service_Entities;
using Catalog_Service.Entities;
using BuildingBlocks.Interfaces;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.CreateCategory
{
    public record CreateCategoryCommand(CreateCategoryDto cat) : IRequest<int>;

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand,int>
    {
        private readonly IBaseRepository<Category> _repo;
        private readonly IUnitOfWork _unitOfWork;
        public CreateCategoryCommandHandler(IBaseRepository<Category> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        public async Task<int>Handle (CreateCategoryCommand request,CancellationToken cancellationToken)
        {
            var dto = request.cat;
            var category = new Category
            {
                Name = dto.Name,
                ImageUrl =dto.ImageUrl,
                ParentCategoryId = dto.ParentCategoryId
            };
           await _repo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return category.Id;

        }

    }




}
