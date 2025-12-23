using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature.ProductExist
{
    public class Handlers : IRequestHandler<ProductExistsQuery, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Product> _productRepo;

        public Handlers(IBaseRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<RequestResponse<bool>> Handle(
            ProductExistsQuery request,
            CancellationToken cancellationToken)
        {
            var exists = await _productRepo
                .Get(p => p.Name == request.Dto.Name)
                .AnyAsync(cancellationToken);

            return RequestResponse<bool>
                .Success(exists, exists ? "Product exists" : "Product does not exist");
        }
    }
}
