using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature.GetProductDetails
{
    public class Handlers : IRequestHandler<GetProductDetailsQuery, RequestResponse<ProductDetailDto>>

    {
        private readonly IBaseRepository<Product> _productRepo;


        public Handlers(IBaseRepository<Product> productRepo) {

            _productRepo = productRepo;


        }
        public async Task<RequestResponse<ProductDetailDto>> Handle(
         GetProductDetailsQuery request,
         CancellationToken cancellationToken)
        {
            var product = await _productRepo
                .Get(p => p.Id == request.ProductId && !p.IsDeleted)
                .Include(p => p.Images)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
                return RequestResponse<ProductDetailDto>
                    .Fail("Product not found");

            // Example tax calculation (14%)
            var tax = product.Price * 0.14m;

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Tax = tax,
                StockQuantity = product.StockQuantity,
                InStock = product.StockQuantity > 0 && product.IsAvailable,
                BrandName = product.Brand?.Name,
                CategoryName = product.Category.Name,
                Images = product.Images
                    .OrderByDescending(i => i.IsMain)
                    .Select(i => i.ImageUrl)
                    .ToList()
            };

            return RequestResponse<ProductDetailDto>.Success(dto);
        }
    }

}

