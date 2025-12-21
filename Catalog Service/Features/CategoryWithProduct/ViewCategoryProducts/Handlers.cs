using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.CategoryWithProduct.ViewCategoryProducts
{
    public class Handlers : IRequestHandler<ViewCategoryProductsQuery,
            RequestResponse<List<DTO>>>
    {
        private readonly IBaseRepository<Product> _productRepo;

        public Handlers(IBaseRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<RequestResponse<List<DTO>>> Handle(
            ViewCategoryProductsQuery request,
            CancellationToken cancellationToken)
        {
            var products = await _productRepo
                .Get(p =>
                    p.CategoryId == request.CategoryId &&
                    p.IsAvailable)
                .Select(p => new DTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    MainImageUrl = p.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return RequestResponse<List<DTO>>
                .Success(products);
        }
    }
}
