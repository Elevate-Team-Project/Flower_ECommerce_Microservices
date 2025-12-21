using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.GetAllCategories
{
    public class Handlers : IRequestHandler<GetAllCategoriesQuery, RequestResponse<List<CategoryViewModel>>>
    {
        private readonly IBaseRepository<Category> _repo;

        public Handlers(IBaseRepository<Category> repo)
        {
            _repo = repo;
        }

        public async Task<RequestResponse<List<CategoryViewModel>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _repo.Get(c => c.IsActive)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync(cancellationToken);

            return RequestResponse<List<CategoryViewModel>>.Success(
                categories,
                "Active categories fetched successfully"
            );
        }
    }

}
