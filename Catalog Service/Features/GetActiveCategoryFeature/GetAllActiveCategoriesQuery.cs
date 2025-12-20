
using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.CategoriesFeature.GetAllCategories;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.GetActiveCategoryFeature
{
    public class GetAllActiveCategoriesQueryHandler : IRequestHandler<GetAllActiveCategoriesQuery, RequestResponse<List<CategoryactiveViewModel>>>
    {
        private readonly IBaseRepository<Category> _repo;
    

        public GetAllActiveCategoriesQueryHandler(IBaseRepository<Category> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;

        }

        public async Task<RequestResponse<List<CategoryactiveViewModel>>> Handle(GetAllActiveCategoriesQuery request, CancellationToken cancellationToken)
        {
         
            var category = _repo.GetAll().Where(c => c.IsActive).ToList();

            if (category == null)
                return RequestResponse<List<CategoryactiveViewModel>>.Fail("No active categories found"); ;


            var categoryVmList = category.Select(c => new CategoryactiveViewModel
            {
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId
            }).ToList();

            return RequestResponse<List<CategoryactiveViewModel>>.Success(categoryVmList, "Fetched successfully");


        }




    }
}
